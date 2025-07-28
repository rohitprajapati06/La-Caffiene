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
        const row = document.querySelector(`tr[data-product-id="${productId}"]`);
        if (!row) {
            showToast('Item not found in cart', 'error');
            return;
        }

        const quantityElement = row.querySelector('.quantity-value');
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
                productId: productId,
                quantity: newQuantity
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

            // Update item total
            if (data.itemTotal !== undefined) {
                row.querySelector('.item-total').textContent = `Rs. ${data.itemTotal}`;
            }

            // Update grand total
            const grandTotalElement = document.querySelector('.cart-grand-total td:last-child');
            if (grandTotalElement && data.grandTotal !== undefined) {
                grandTotalElement.textContent = `Rs. ${data.grandTotal}`;
            }

            showToast('Cart updated');
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
                productId: productId
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data.success) {
            // Remove row from table
            const row = document.querySelector(`tr[data-product-id="${productId}"]`);
            if (row) row.remove();

            // Update cart count
            updateCartCount(data.count);

            // Update grand total
            const grandTotalElement = document.querySelector('.cart-grand-total td:last-child');
            if (grandTotalElement && data.grandTotal !== undefined) {
                grandTotalElement.textContent = `Rs. ${data.grandTotal}`;
            }

            showToast('Item removed from cart');

            // If cart is empty, show empty message
            if (data.count === 0) {
                const cartTable = document.querySelector('.cart-table');
                if (cartTable) {
                    const emptyMessage = document.createElement('div');
                    emptyMessage.className = 'alert alert-info mt-4 text-center';
                    emptyMessage.textContent = 'Your cart is empty.';
                    cartTable.parentNode.insertBefore(emptyMessage, cartTable.nextSibling);
                    cartTable.remove();
                }
            }
        } else {
            showToast(data.message || 'Failed to remove item', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
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