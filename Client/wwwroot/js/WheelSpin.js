// WheelSpin.razor.js (ES module)

const instances = new Map();

const norm = a => ((a % 360) + 360) % 360;
const easeOutQuint = t => 1 - Math.pow(1 - t, 5);
const easeOutSine = t => Math.sin((t * Math.PI) / 2);
const shortestDelta = (from, to) => {
  let d = ((to - from + 540) % 360) - 180;
  return d === -180 ? 180 : d;
};

function readSize(el) {
  const v = parseFloat(getComputedStyle(el).getPropertyValue('--size'));
  return (Number.isFinite(v) && v > 0) ? v : (el.clientWidth || el.offsetWidth || 320);
}

function gradientFor(sectors, sectorAngle, colors) {
  const palette = (Array.isArray(colors) && colors.length > 0)
    ? colors.filter(Boolean)
    : ['#ef4444', '#10b981', '#3b82f6', '#f59e0b', '#8b5cf6', '#ec4899'];

  let g = 'conic-gradient(from -90deg';
  for (let i = 0; i < sectors; i++) {
    const start = i * sectorAngle, end = (i + 1) * sectorAngle;
    const color = palette[i % palette.length];
    g += `, ${color} ${start}deg ${end}deg`;
  }
  g += ')';
  return g;
}

function bump(pointer) {
  pointer.classList.remove('bump');
  void pointer.offsetWidth;
  pointer.classList.add('bump');
}

export function initWheel(rootEl, wheelId, pinsId, pointerId, labels, colors, dotNetRef) {
  // Use the passed element ref as the wheel (critical for clipping/layout)
  const wheel = rootEl;
  const pinsEl = document.getElementById(pinsId);
  const pointer = document.getElementById(pointerId);
  if (!wheel || !pinsEl || !pointer) { console.error('Wheel elements not found'); return; }

  // Make pins a child of the wheel so it rotates with it
  if (pinsEl.parentElement !== wheel) {
    wheel.appendChild(pinsEl);
  }
  // Ensure correct sizing/positioning in this new context
  Object.assign(pinsEl.style, {
    position: 'absolute',
    inset: '0',
    pointerEvents: 'none',
    zIndex: '2'
  });
  // Tag elements with the Blazor scope attribute so isolated CSS applies
  const scopeAttr = [...wheel.attributes].find(a => a.name.startsWith('b-'))?.name;
  const tag = (el) => { if (scopeAttr) el.setAttribute(scopeAttr, ''); return el; };

  const sectors = labels.length;
  if (!sectors) return;
  const sectorAngle = 360 / sectors;
  const half = sectorAngle / 2;

  // Background
  wheel.style.background = gradientFor(sectors, sectorAngle, colors);

  // Labels + tokens (inside the wheel so overflow:hidden clips them)
  function renderLabels() {
    const size = parseFloat(getComputedStyle(wheel).getPropertyValue('--size')) || wheel.clientWidth || 320;
    const radius = size / 2 - 42;
    wheel.querySelectorAll('.wheel-label, .wheel-token').forEach(el => el.remove());

    for (let i = 0; i < sectors; i++) {
      const angle = -90 + i * sectorAngle + half;

      const labelEl = tag(document.createElement('div'));
      Object.assign(labelEl.style, {
        position: 'absolute', left: '50%', top: '50%',
        transformOrigin: '0 0', pointerEvents: 'none', userSelect: 'none', zIndex: '2'
      });
      labelEl.className = 'wheel-label';
      labelEl.textContent = labels[i];
      labelEl.style.transform =
        `translate(-50%,-50%) rotate(${angle}deg) translate(0, -${radius}px) rotate(${-angle}deg)`;
      wheel.appendChild(labelEl);

      const tokenEl = tag(document.createElement('div'));
      Object.assign(tokenEl.style, {
        position: 'absolute', left: '50%', top: '50%',
        transformOrigin: '0 0', pointerEvents: 'none', userSelect: 'none', zIndex: '2'
      });
      tokenEl.className = 'wheel-token';
      tokenEl.textContent = '🪙';
      tokenEl.style.transform =
        `translate(-50%,-50%) rotate(${angle}deg) translate(0, -${radius - 22}px) rotate(${-angle}deg)`;
      wheel.appendChild(tokenEl);
    }
  }

  // Pins at boundaries (where two slices meet)
  function renderPins() {
    const size = parseFloat(getComputedStyle(wheel).getPropertyValue('--size')) || wheel.clientWidth || 320;
    const R = size / 2 - 8;
    pinsEl.innerHTML = '';

    for (let i = 0; i < sectors; i++) {
      const deg = -90 + i * sectorAngle;                 // ← boundary (no +half)
      const pin = tag(document.createElement('div'));
      pin.className = 'pin';
      pin.style.position = 'absolute';
      pin.style.left = '50%';
      pin.style.top = '50%';
      pin.style.transform = `translate(-50%,-50%) rotate(${deg}deg) translate(0, -${R}px)`;
      pin.style.width = '10px';
      pin.style.height = '10px';
      pin.style.background = '#e5e7eb';
      pin.style.border = '2px solid rgba(0,0,0,.15)';
      pin.style.borderRadius = '50%';
      pin.style.boxShadow = '0 1px 2px rgba(0,0,0,.2), inset 0 1px 0 rgba(255,255,255,.8)';
      pin.style.pointerEvents = 'none';
      pin.style.zIndex = '2';
      pinsEl.appendChild(pin);
    }
  }

  renderLabels();
  renderPins();

  let current = 0;
  wheel.style.transform = `rotate(${current}deg)`;

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
      if (idxTick !== lastTick) { lastTick = idxTick; bump(pointer); }

      if (t < 1) { requestAnimationFrame(frame); return; }

      // --- Snap so the pointer is at the CENTER of the winning slice ---
      const final = ((current % 360) + 360) % 360;
      const c = ((-final % 360) + 360) % 360;
      const i = Math.round(((c + 90 - half) % 360 + 360) % 360 / sectorAngle) % sectors;
      const centerRotation = ((90 - i * sectorAngle - half) % 360 + 360) % 360;
      const delta = ((centerRotation - final + 540) % 360) - 180;  // shortestDelta


      if (Math.abs(delta) > 0.001) {
        const snapStart = performance.now();
        const snapFrom = final;
        const dur = 180;
        const snapFrame = (now2) => {
          const tt = Math.min(1, (now2 - snapStart) / dur);
          const easedSnap = easeOutSine(tt);
          current = snapFrom + delta * easedSnap;
          wheel.style.transform = `rotate(${current}deg)`;
          if (tt < 1) requestAnimationFrame(snapFrame);
          else { current = norm(current); wheel.style.transform = `rotate(${current}deg)`; callback(labels[i]); }
        };
        requestAnimationFrame(snapFrame);
      } else {
        current = final;
        wheel.style.transform = `rotate(${current}deg)`;
        callback(labels[i]);
      }
    }
    requestAnimationFrame(frame);
  }

  // store instance with center-targeted spin
  instances.set(wheelId, {
    spin() {
      const k = Math.floor(Math.random() * sectors);
      const fullTurns = 2 + Math.floor(Math.random() * 3);
      const baseMod = ((current % 360) + 360) % 360;
      const targetMod = ((90 - k * sectorAngle - half) % 360 + 360) % 360; // center
      const deltaToTarget = ((targetMod - baseMod + 360) % 360);
      const totalDelta = fullTurns * 360 + deltaToTarget;


      spinTo(totalDelta, 3400, (winningLabel) => {
        dotNetRef.invokeMethodAsync('HandleSpinComplete', winningLabel);
      });
    }
  });
}

export function spinWheel(wheelId) {
  const inst = instances.get(wheelId);
  if (inst) inst.spin();
}

export function dispose() {
  instances.clear();
}
