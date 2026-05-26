// E-Store Site JavaScript

// Safely escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Update cart count badge via AJAX
function updateCartCount() {
    const isAuthenticated = document.body.dataset.authenticated === 'true';
    if (!isAuthenticated) return;

    const url = document.body.dataset.cartCountUrl;
    if (!url) return;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            const badges = document.querySelectorAll('.cart-count-badge');
            badges.forEach(badge => {
                if (data.count > 0) {
                    badge.textContent = data.count;
                    badge.style.display = 'flex';
                } else {
                    badge.style.display = 'none';
                }
            });
        })
        .catch(() => { /* silently fail for unauthenticated users */ });
}

// Show toast notification
function showNotification(message, type = 'success') {
    const safeMessage = escapeHtml(message);
    const bgColor = type === 'success' ? 'linear-gradient(135deg, #10b981, #059669)'
        : type === 'error' ? 'linear-gradient(135deg, #ef4444, #dc2626)'
        : 'linear-gradient(135deg, #667eea, #764ba2)';

    const notification = document.createElement('div');
    notification.style.cssText = `position:fixed;top:20px;right:20px;padding:16px 24px;background:${bgColor};color:white;border-radius:12px;box-shadow:0 8px 24px rgba(0,0,0,0.15);z-index:10000;font-weight:500;font-size:0.95rem;transform:translateX(120%);transition:transform 0.4s cubic-bezier(0.4,0,0.2,1);max-width:400px;`;
    notification.innerHTML = `<i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'} me-2"></i>${safeMessage}`;
    document.body.appendChild(notification);
    requestAnimationFrame(() => { notification.style.transform = 'translateX(0)'; });
    setTimeout(() => {
        notification.style.transform = 'translateX(120%)';
        setTimeout(() => notification.remove(), 400);
    }, 3000);
}

// AJAX Add to Cart
function addToCartAjax(productId) {
    const url = document.body.dataset.addToCartUrl;
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!url) return;

    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId: productId, quantity: 1 })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification(data.message || 'Added to cart!');
            updateCartCount();
        } else {
            showNotification(data.message || 'Failed to add to cart', 'error');
        }
    })
    .catch(() => showNotification('Failed to add to cart', 'error'));
}

// AJAX Add to Cart with quantity
function addToCartAjaxWithQuantity(productId, quantityInputId) {
    const qty = parseInt(document.getElementById(quantityInputId)?.value || '1');
    const url = document.body.dataset.addToCartUrl;
    if (!url) return;

    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId: productId, quantity: qty })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification(data.message || 'Added to cart!');
            updateCartCount();
        } else {
            showNotification(data.message || 'Failed to add to cart', 'error');
        }
    })
    .catch(() => showNotification('Failed to add to cart', 'error'));
}

// Initialize on DOM load
document.addEventListener('DOMContentLoaded', function () {
    updateCartCount();
});
