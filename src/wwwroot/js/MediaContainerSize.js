let elementId, dotNet, timeout;

export function initialize(dotNetObj, elId) {
    dotNet = dotNetObj;
    elementId = elId;

    reportElementSize();
}

function reportElementSize() {
    let el = document.getElementById(elementId);
    if (el) {
        let vw = document.documentElement.clientWidth;
        let dpr = window.devicePixelRatio;
        dotNet.invokeMethodAsync("ElementChanged", el.clientWidth, el.clientHeight, vw, dpr);
    }
}

function queueReportElementSize() {
    clearTimeout(timeout);
    timeout = setTimeout(reportElementSize, 500);
}

window.onresize = queueReportElementSize;