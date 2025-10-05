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
        const size = parseFloat(getComputedStyle(wheel).getPropertyValue('--size')) || 320;
        const radius = size / 2 - 42;

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
        const size = parseFloat(getComputedStyle(wheel).getPropertyValue('--size')) || 320;
        const R = size / 2 + 4;
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
    wheel.style.transform = `rotate(${current}deg)`;

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
            wheel.style.transform = `rotate(${current}deg)`;

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
                        wheel.style.transform = `rotate(${current}deg)`;
                        if (tt < 1) {
                            requestAnimationFrame(snapFrame);
                        } else {
                            current = norm(current);
                            wheel.style.transform = `rotate(${current}deg)`;
                            callback(labels[i]);
                        }
                    };
                    requestAnimationFrame(snapFrame);
                } else {
                    current = final;
                    wheel.style.transform = `rotate(${current}deg)`;
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
