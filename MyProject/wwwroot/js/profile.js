// Time-based quotes
const timeQuotes = {
    morning: [
        "Start your day deliciously—breakfast served hot and fresh!",
        "Breakfast: The secret ingredient to a brighter morning.",
        "Wake up, eat well, and conquer your day."
    ],
    afternoon: [
        "Sip, savor, and unwind—afternoon delights await.",
        "Refresh your senses with our signature drinks.",
        "Cheers to afternoons filled with great flavors and good company."
    ],
    evening: [
        "Evenings made better with tasty bites and good times.",
        "Snack o'clock: indulge, relax, repeat.",
        "Perfect snacks for those sunset cravings."
    ],
    night: [
        "End your day with a feast—dinner done right.",
        "Savor the night, one delicious bite at a time.",
        "Dinner is not just a meal, it's an experience."
    ]
};

// Function to get current time period
function getTimePeriod() {
    const hour = new Date().getHours();
    if (hour >= 5 && hour < 12) return 'morning';
    if (hour >= 12 && hour < 17) return 'afternoon';
    if (hour >= 17 && hour < 21) return 'evening';
    return 'night';
}

// Function to get appropriate grid based on time
function getTimeBasedGrid() {
    const hour = new Date().getHours();
    if (hour >= 5 && hour < 12) return 'Grid1';   // Morning
    if (hour >= 12 && hour < 17) return 'Grid2';  // Afternoon
    if (hour >= 17 && hour < 21) return 'Grid3';  // Evening
    return 'Grid4';                               // Night
}

// Function to rotate quotes
function rotateQuotes() {
    const period = getTimePeriod();
    const quotes = timeQuotes[period];
    const container = document.getElementById('quoteContainer');

    // Clear existing quotes
    container.innerHTML = '';

    // Create quote elements
    quotes.forEach((quote, index) => {
        const quoteElement = document.createElement('div');
        quoteElement.className = 'profile-quote';
        quoteElement.textContent = quote;
        quoteElement.style.display = index === 0 ? 'block' : 'none';
        quoteElement.style.opacity = index === 0 ? '1' : '0';
        container.appendChild(quoteElement);
    });

    // Start rotation
    let currentIndex = 0;
    const quoteElements = container.getElementsByClassName('profile-quote');

    setInterval(() => {
        // Fade out current quote
        quoteElements[currentIndex].style.opacity = '0';
        quoteElements[currentIndex].style.display = 'none';

        // Move to next quote
        currentIndex = (currentIndex + 1) % quotes.length;

        // Fade in next quote
        setTimeout(() => {
            quoteElements[currentIndex].style.display = 'block';
            setTimeout(() => {
                quoteElements[currentIndex].style.opacity = '1';
            }, 50);
        }, 800);
    }, 3000);
}

function openTab(evt, tabName) {
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tabcontent");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }
    tablinks = document.getElementsByClassName("tablinks");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }
    document.getElementById(tabName).style.display = "block";

    // Don't add 'active' class to logout button
    if (evt.currentTarget.type !== 'submit') {
        evt.currentTarget.className += " active";
    }
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function () {
    rotateQuotes();
    document.getElementById("defaultOpen").click();

    // Update button text based on time
    const period = getTimePeriod();
    const btn = document.getElementById('menuRedirectBtn');
    if (period === 'morning') {
        btn.textContent = 'Breakfast Menu';
    } else if (period === 'afternoon') {
        btn.textContent = 'Lunch Menu';
    } else if (period === 'evening') {
        btn.textContent = 'Dinner Menu';
    } else {
        btn.textContent = 'Late Night Menu';
    }
    btn.innerHTML += ' <i class="fas fa-arrow-right"></i>';

    // Add click event for menu redirect
    btn.addEventListener('click', function () {
        const grid = getTimeBasedGrid();
        window.location.href = '/#Menu';
    });
});