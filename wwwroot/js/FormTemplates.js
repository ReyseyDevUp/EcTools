function loading(size) {
    document.querySelectorAll(".bar").forEach(function (current) {
        let startWidth = 0;
        // const endWidth = current.dataset.size;
        const endWidth = size;

        /* 
        setInterval() time sholud be set as trasition time / 100. 
        In our case, 2 seconds / 100 = 20 milliseconds. 
        */
        const interval = setInterval(frame, 20);

        if (Number(current.firstElementChild.innerText.split('%')[0]) > 0) {
            startWidth = Number(current.firstElementChild.innerText.split('%')[0]);
        }

        function frame() {
            if (startWidth >= endWidth) {
                clearInterval(interval);
            } else {
                startWidth++;
                current.style.width = `${endWidth}%`;
                current.firstElementChild.innerText = `${startWidth}%`;
            }
        }
    });
}

function getWarningPane(title, description) {
    return `
        <div class="EC_PROCESS_WARNING_STATE_CONTAINER EC_WARNING">
            <div class="EC_PROCESS_WARNING_STATE_HEADER_CONTAINER">
                <div class="EC_PROCESS_WARNING_STATE_HEADER_ICON_CONTAINER">
                    <img src="~/media/images/warning.png" alt="WARNING: " />
                </div>
                <div class="EC_PROCESS_WARNING_STATE_HEADER_TITLE_CONTAINER">
                    <h1>${title}</h1>
                </div>
            </div>
            <div class="EC_PROCESS_WARNING_STATE_BODY_CONTAINER">
                <p>
                    ${description}
                </p>
            </div>
        </div>
    `;
}

function getErrorPane(title, description) {
    return `
        <div class="EC_PROCESS_ERROR_STATE_CONTAINER EC_WARNING">
            <div class="EC_PROCESS_ERROR_STATE_HEADER_CONTAINER">
                <div class="EC_PROCESS_ERROR_STATE_HEADER_ICON_CONTAINER">
                    <img src="~/media/images/check.png" alt="WARNING: " />
                </div>
                <div class="EC_PROCESS_ERROR_STATE_HEADER_TITLE_CONTAINER">
                    <h1>${title}</h1>
                </div>
            </div>
            <div class="EC_PROCESS_ERROR_STATE_BODY_CONTAINER">
                <p>
                    ${description}
                </p>
            </div>
        </div>
    `;
}
function getSuccessPane(title, description) {
    return `
        <div class="EC_PROCESS_SUCCESS_STATE_CONTAINER EC_WARNING">
            <div class="EC_PROCESS_SUCCESS_STATE_HEADER_CONTAINER">
                <div class="EC_PROCESS_SUCCESS_STATE_HEADER_ICON_CONTAINER">
                    <img src="~/media/images/check.png" alt="WARNING: " />
                </div>
                <div class="EC_PROCESS_SUCCESS_STATE_HEADER_TITLE_CONTAINER">
                    <h1>${title}</h1>
                </div>
            </div>
            <div class="EC_PROCESS_SUCCESS_STATE_BODY_CONTAINER">
                <p>
                    ${description}
                </p>
            </div>
        </div>
    `;
}