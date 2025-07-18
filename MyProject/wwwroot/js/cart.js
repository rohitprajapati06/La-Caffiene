
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
