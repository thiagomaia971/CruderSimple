function isDevice() {
    return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini|mobile/i.test(navigator.userAgent);
}

window.getWindowDimensions = function () {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
}; 

// listen for page resize
function resizeListener(dotnethelper) {
    $(window).resize(() => {
        let browserHeight = $(window).innerHeight();
        let browserWidth = $(window).innerWidth();
        dotnethelper.invokeMethodAsync('SetBrowserDimensions', browserWidth, browserHeight).then(() => {
            // success, do nothing
        }).catch(error => {
            console.log("Error during browser resize: " + error);
        });
    });
}