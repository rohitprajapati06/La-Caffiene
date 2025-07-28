document.addEventListener('DOMContentLoaded', function () {
    // Handle add to cart button clicks
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('add-to-cart-btn') || e.target.closest('.add-to-cart-btn')) {
            e.preventDefault();
            const button = e.target.classList.contains('add-to-cart-btn') ?
                e.target : e.target.closest('.add-to-cart-btn');

            addToCart(
                button.dataset.productId,
                button.dataset.name,
                button.dataset.price,
                button.dataset.image
            );
        }

        // Quantity minus button
        if (e.target.classList.contains('minus') || e.target.closest('.minus')) {
            e.preventDefault();
            const button = e.target.classList.contains('minus') ?
                e.target : e.target.closest('.minus');
            const productId = parseInt(button.dataset.productId);
            updateQuantity(productId, -1);
        }

        // Quantity plus button
        if (e.target.classList.contains('plus') || e.target.closest('.plus')) {
            e.preventDefault();
            const button = e.target.classList.contains('plus') ?
                e.target : e.target.closest('.plus');
            const productId = parseInt(button.dataset.productId);
            updateQuantity(productId, 1);
        }

        // Remove item button
        if (e.target.classList.contains('remove-item-btn') || e.target.closest('.remove-item-btn')) {
            e.preventDefault();
            const button = e.target.classList.contains('remove-item-btn') ?
                e.target : e.target.closest('.remove-item-btn');
            const productId = parseInt(button.dataset.productId);
            removeItem(productId);
        }
    });

    // Coupon application
    document.getElementById('applyCouponBtn')?.addEventListener('click', applyCoupon);

    // Allow pressing Enter in coupon input field
    document.getElementById('couponCode')?.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            applyCoupon();
        }
    });
});

async function addToCart(productId, productName, price, image) {
    console.log('Attempting to add to cart:', { productId, productName, price, image });

    try {
        const product = {
            ProductId: parseInt(productId),
            ProductName: productName,
            Price: parseInt(price),
            Image: image,
            Quantity: 1
        };

        // Get the anti-forgery token
        const token = document.querySelector('#__RequestVerificationToken')?.value;

        const response = await fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(product)
        });

        console.log('Response status:', response.status);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Response data:', data);

        if (data.success) {
            updateCartCount(data.count);
            showToast('Item added to cart!');
        } else {
            showToast(data.message || 'Failed to add item to cart', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('An error occurred. Please try again.', 'error');

        if (error.message.includes('401')) {
            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }
    }
}

async function updateQuantity(productId, change) {
    try {
        const item = document.querySelector(`.cart-item[data-product-id="${productId}"]`);
        if (!item) {
            showToast('Item not found in cart', 'error');
            return;
        }

        const quantityElement = item.querySelector('.quantity-value');
        let currentQuantity = parseInt(quantityElement.textContent);
        let newQuantity = currentQuantity + change;

        // Ensure quantity doesn't go below 1
        newQuantity = Math.max(1, newQuantity);

        const token = document.querySelector('#__RequestVerificationToken').value;

        const response = await fetch('/Cart/UpdateCartItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                ProductId: productId,
                Quantity: newQuantity
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data.success) {
            // Update UI
            quantityElement.textContent = newQuantity;
            updateCartCount(data.count);

            // Update item subtotal
            if (data.itemTotal !== undefined) {
                item.querySelector('.item-subtotal').textContent = `Rs. ${data.itemTotal}`;
            }

            // Update grand total
            const grandTotalElement = document.querySelector('.grand-total span:last-child');
            if (grandTotalElement && data.grandTotal !== undefined) {
                grandTotalElement.textContent = `Rs. ${data.grandTotal}`;
            }

            showToast('Cart Updated');
        } else {
            showToast(data.message || 'Failed to update cart', 'error');
            // Revert to previous quantity if update failed
            quantityElement.textContent = currentQuantity;
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('An error occurred. Please try again.', 'error');
    }
}

async function removeItem(productId) {
    try {
        const token = document.querySelector('#__RequestVerificationToken').value;

        const response = await fetch('/Cart/RemoveCartItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                ProductId: productId
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data.success) {
            // Remove item from UI
            const item = document.querySelector(`.cart-item[data-product-id="${productId}"]`);
            if (item) item.remove();

            // Update cart count
            updateCartCount(data.count);

            // Update grand total
            const grandTotalElement = document.querySelector('.grand-total span:last-child');
            if (grandTotalElement && data.grandTotal !== undefined) {
                grandTotalElement.textContent = `Rs. ${data.grandTotal}`;
            }

            showToast('Item Removed');

            // If cart is empty, reload the page to show empty message
            if (data.count === 0) {
                window.location.reload();
            }
        } else {
            showToast(data.message || 'Failed to remove item', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('An error occurred. Please try again.', 'error');
    }
}

async function applyCoupon() {
    const couponCode = document.getElementById('couponCode').value.trim();
    const couponMessage = document.getElementById('couponMessage');

    if (!couponCode) {
        couponMessage.textContent = 'Please enter a coupon code';
        couponMessage.className = 'coupon-message error';
        return;
    }

    try {
        const token = document.querySelector('#__RequestVerificationToken').value;

        const response = await fetch('/Cart/ApplyCoupon', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                CouponCode: couponCode
            })
        });

        const data = await response.json();

        if (data.success) {
            couponMessage.textContent = data.message;
            couponMessage.className = 'coupon-message success';

            // Update the UI with new totals
            const grandTotalElement = document.querySelector('.grand-total span:last-child');
            if (grandTotalElement) {
                grandTotalElement.textContent = `Rs. ${data.grandTotal}`;
            }

            // Update discount display
            const discountElement = document.querySelector('.discount-row span:last-child');
            if (discountElement) {
                discountElement.textContent = `- Rs. ${data.discount}`;
            }

            // Update item display if needed
            if (data.affectedItemId) {
                const item = document.querySelector(`.cart-item[data-product-id="${data.affectedItemId}"]`);
                if (item) {
                    const subtotalElement = item.querySelector('.item-subtotal');
                    if (subtotalElement) {
                        subtotalElement.textContent = `Rs. ${data.itemTotal}`;
                    }
                }
            }

            showToast(data.message);
        } else {
            couponMessage.textContent = data.message;
            couponMessage.className = 'coupon-message error';
            showToast(data.message, 'error');
        }
    } catch (error) {
        console.error('Error applying coupon:', error);
        couponMessage.textContent = 'An error occurred. Please try again.';
        couponMessage.className = 'coupon-message error';
        showToast('An error occurred. Please try again.', 'error');
    }
}

function updateCartCount(count) {
    const cartCountElements = document.querySelectorAll('.cart-count');
    cartCountElements.forEach(element => {
        element.textContent = count;
        element.classList.add('pulse');
        setTimeout(() => element.classList.remove('pulse'), 500);
    });
}

function showToast(message, type = 'success') {
    const existingToasts = document.querySelectorAll('.toast');
    existingToasts.forEach(toast => toast.remove());

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <div class="toast-message">${message}</div>
    `;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('fade-out');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}