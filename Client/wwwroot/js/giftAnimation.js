window.giftAnimation = {
    animateToInventory: function (sourceSelector, giftId) {
        const sourceEl = document.querySelector(`[data-gift-id="${giftId}"] .gift-card-image`);
        const targetEl = document.querySelector('.token-balance') || document.querySelector('.inventory-target');
        
        if (!sourceEl || !targetEl) {
            console.log('Animation elements not found');
            return;
        }
        
        const sourceRect = sourceEl.getBoundingClientRect();
        const targetRect = targetEl.getBoundingClientRect();
        
        const clone = sourceEl.cloneNode(true);
        clone.classList.add('gift-flying');
        clone.style.position = 'fixed';
        clone.style.left = sourceRect.left + 'px';
        clone.style.top = sourceRect.top + 'px';
        clone.style.width = sourceRect.width + 'px';
        clone.style.height = sourceRect.height + 'px';
        clone.style.zIndex = '9999';
        clone.style.pointerEvents = 'none';
        clone.style.borderRadius = '12px';
        clone.style.objectFit = 'contain';
        
        document.body.appendChild(clone);
        
        const translateX = targetRect.left - sourceRect.left + (targetRect.width / 2) - (sourceRect.width / 2);
        const translateY = targetRect.top - sourceRect.top + (targetRect.height / 2) - (sourceRect.height / 2);
        const scale = 0.3;
        
        requestAnimationFrame(() => {
            clone.style.transition = 'transform 0.6s cubic-bezier(0.25, 0.46, 0.45, 0.94), opacity 0.6s ease';
            clone.style.transform = `translate(${translateX}px, ${translateY}px) scale(${scale})`;
            clone.style.opacity = '0.8';
        });
        
        setTimeout(() => {
            clone.style.opacity = '0';
            clone.style.transform += ' scale(0)';
        }, 500);
        
        setTimeout(() => {
            if (clone.parentNode) {
                clone.parentNode.removeChild(clone);
            }
        }, 700);
    }
};
