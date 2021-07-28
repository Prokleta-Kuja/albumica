let elementId, dotNet, timeout;

export function initialize(dotNetObj, elId) {
    dotNet = dotNetObj;
    elementId = elId;
    reportElementSize()
}

function reportElementSize() {
    let el = document.getElementById(elementId);
    if (el) {
        el.childNodes[0].src = "";
        dotNet.invokeMethodAsync("ElementChanged", el.clientWidth, el.clientHeight, window.devicePixelRatio);
    }
}

function queueReportElementSize() {
    clearTimeout(timeout);
    timeout = setTimeout(reportElementSize, 500);
}

window.onresize = queueReportElementSize;