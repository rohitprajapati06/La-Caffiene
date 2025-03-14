function toggleMenu(x) {
    x.classList.toggle("change"); // Toggle hamburger to 'X'
    document.getElementById("mySidebar").classList.toggle("active"); // Show/hide sidebar
}



// Handle coupon slider
let couponSlideIndex = 0;

function slide(direction) {
    const slider = document.getElementById('slider');
    const coupons = document.querySelectorAll('.coupon');
    const couponWidth = coupons[0].offsetWidth + 20; // Adjust gap as needed

    couponSlideIndex += direction;

    if (couponSlideIndex < 0) {
        couponSlideIndex = coupons.length - 1;
    } else if (couponSlideIndex >= coupons.length) {
        couponSlideIndex = 0;
    }

    slider.style.transform = `translateX(${-couponSlideIndex * couponWidth}px)`;
}


/*

        Menu 
*/

function openPage(evt, pageName) {
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tab-content");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].classList.remove("active");
    }
    tablinks = document.getElementsByClassName("pagination")[0].getElementsByTagName("button");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].classList.remove("active");
    }
    document.getElementById(pageName).classList.add("active");
    evt.currentTarget.classList.add("active");
}