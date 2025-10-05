// Wheel spin implementation for Blazor
window.wheelInstances = {};

window.initWheel = function (wheelId, pinsId, pointerId, labels, colors, dotNetRef) {
    const wheel = document.getElementById(wheelId);
    const pinsEl = document.getElementById(pinsId);
    const pointer = document.getElementById(pointerId);

    if (!wheel || !pinsEl || !pointer) {
        console.error('Wheel elements not found');
        return;
    }

    // Force wheel to be circular with inline styles
    const size = window.innerWidth < 480 ? 280 : 320;
    wheel.style.setProperty('--size', size + 'px');
    wheel.style.width = size + 'px';
    wheel.style.height = size + 'px';
    wheel.style.borderRadius = '50%';
    wheel.style.position = 'relative';
    wheel.style.transform = 'none';
    wheel.style.transition = 'none !important';
    wheel.style.willChange = 'transform';
    wheel.style.boxShadow = '0 6px 20px rgba(0,0,0,.15), inset 0 0 0 6px rgba(255,255,255,.7)';
    wheel.style.zIndex = '1';

    const sectors = labels.length;
    const sectorAngle = 360 / sectors;

    let current = 0;

    const norm = a => ((a % 360) + 360) % 360;
    const easeOutQuint = t => 1 - Math.pow(1 - t, 5);
    const easeOutSine = t => Math.sin((t * Math.PI) / 2);
    const shortestDelta = (from, to) => {
        let d = ((to - from + 540) % 360) - 180;
        return d === -180 ? 180 : d;
    };

    function generateConicGradient() {
        const stops = colors.map((color, i) => {
            const startDeg = i * sectorAngle;
            const endDeg = (i + 1) * sectorAngle;
            return `${color} ${startDeg}deg ${endDeg}deg`;
        }).join(', ');
        return `conic-gradient(from -90deg, ${stops})`;
    }

    wheel.style.background = generateConicGradient();

    function renderLabels() {
        const wheelSize = parseFloat(getComputedStyle(wheel).getPropertyValue('--size'));
        const radius = wheelSize / 2 - 42;

        wheel.querySelectorAll('.wheel-label').forEach(el => el.remove());

        for (let i = 0; i < sectors; i++) {
            const angle = -sectorAngle/2 + i * sectorAngle;
            const el = document.createElement('div');
            el.className = 'wheel-label';
            el.textContent = labels[i];
            el.style.transform =
                `translate(-50%, -50%) rotate(${angle}deg) translate(0, -${radius}px) rotate(${-angle}deg)`;
            wheel.appendChild(el);
        }
    }

    function renderPins() {
        const wheelSize = parseFloat(getComputedStyle(wheel).getPropertyValue('--size'));
        const R = wheelSize / 2 + 4;
        pinsEl.innerHTML = '';
        for (let i = 0; i < sectors; i++) {
            const deg = -90 + i * sectorAngle;
            const pin = document.createElement('div');
            pin.className = 'pin';
            pin.style.left = '50%';
            pin.style.top = '50%';
            pin.style.transform = `translate(-50%,-50%) rotate(${deg}deg) translate(0, -${R}px)`;
            pinsEl.appendChild(pin);
        }
    }

    renderLabels();
    renderPins();

    current = 0;
    const initialTransform = `rotate(${current}deg)`;
    wheel.style.transform = initialTransform;
    wheel.style.WebkitTransform = initialTransform;
    wheel.style.MozTransform = initialTransform;

    function bumpPointer() {
        pointer.classList.remove('bump');
        void pointer.offsetWidth;
        pointer.classList.add('bump');
    }

    function spinTo(deltaDeg, durationMs, callback) {
        const start = performance.now();
        const startAbs = current;

        const baseMod = norm(startAbs);
        let lastTick = Math.floor(baseMod / sectorAngle);

        function frame(now) {
            const t = Math.min(1, (now - start) / durationMs);
            const eased = easeOutQuint(t);
            const angle = startAbs + deltaDeg * eased;

            current = angle;
            const rotateTransform = `rotate(${current}deg)`;
            wheel.style.transform = rotateTransform;
            wheel.style.WebkitTransform = rotateTransform;
            wheel.style.MozTransform = rotateTransform;

            const progressed = angle - startAbs;
            const idxTick = Math.floor((baseMod + progressed + 1e-6) / sectorAngle);
            if (idxTick !== lastTick) {
                lastTick = idxTick;
                bumpPointer();
            }

            if (t < 1) {
                requestAnimationFrame(frame);
            } else {
                const final = norm(current);
                const c = norm(-final);
                const i = Math.round((c + sectorAngle) / sectorAngle) % sectors;
                const centerRotation = norm(sectorAngle - i * sectorAngle);
                const delta = shortestDelta(final, centerRotation);

                if (Math.abs(delta) > 0.001) {
                    const snapStart = performance.now();
                    const snapFrom = final;
                    const dur = 180;

                    const snapFrame = (now2) => {
                        const tt = Math.min(1, (now2 - snapStart) / dur);
                        const easedSnap = easeOutSine(tt);
                        current = snapFrom + delta * easedSnap;
                        const snapTransform1 = `rotate(${current}deg)`;
                        wheel.style.transform = snapTransform1;
                        wheel.style.WebkitTransform = snapTransform1;
                        wheel.style.MozTransform = snapTransform1;
                        if (tt < 1) {
                            requestAnimationFrame(snapFrame);
                        } else {
                            current = norm(current);
                            const snapTransform2 = `rotate(${current}deg)`;
                            wheel.style.transform = snapTransform2;
                            wheel.style.WebkitTransform = snapTransform2;
                            wheel.style.MozTransform = snapTransform2;
                            callback(labels[i]);
                        }
                    };
                    requestAnimationFrame(snapFrame);
                } else {
                    current = final;
                    const finalTransform = `rotate(${current}deg)`;
                    wheel.style.transform = finalTransform;
                    wheel.style.WebkitTransform = finalTransform;
                    wheel.style.MozTransform = finalTransform;
                    callback(labels[i]);
                }
            }
        }
        requestAnimationFrame(frame);
    }

    wheelInstances[wheelId] = {
        spin: function() {
            const k = Math.floor(Math.random() * sectors);
            const fullTurns = 2 + Math.floor(Math.random() * 3);
            const baseMod = norm(current);
            const targetMod = norm(sectorAngle - k * sectorAngle);
            const deltaToTarget = norm(targetMod - baseMod);
            const totalDelta = fullTurns * 360 + deltaToTarget;
            
            spinTo(totalDelta, 3400, (winningLabel) => {
                dotNetRef.invokeMethodAsync('HandleSpinComplete', winningLabel);
            });
        }
    };
};

window.spinWheel = function (wheelId) {
    if (wheelInstances[wheelId]) {
        wheelInstances[wheelId].spin();
    }
};
