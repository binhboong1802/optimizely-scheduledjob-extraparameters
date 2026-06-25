// Base route of the protected module's controller (shell route, independent of the admin UI path).
const EXTRA_PARAMS_API_BASE = '/episerver/OptiScheduledJob.ExtraParameters/ScheduledJobExtraParameters';

// The admin UI base path (everything before the SPA hash). Derived from the current URL instead of
// hardcoding "/EPiServer/..." so it keeps working when the admin path changes between CMS versions.
function getAdminBaseUrl() {
    return window.location.href.split('#')[0];
}

document.addEventListener('DOMContentLoaded', function () {
    const rootDiv = document.getElementById('root');
    const sideBarNavigationRoot = document.getElementById('sideBarNavigationRoot');
    const observerConfig = { childList: true, subtree: true };

    const rootContentObserver = new MutationObserver((mutationsList) => {
        for (const mutation of mutationsList) {
            if (mutation.type === 'childList') {
                const scheduledJobSettings = mutation.target.querySelector('.scheduled-job.settings');
                if (scheduledJobSettings) {
                    ScheduledJobExtra.addScheduledJobSettingsClickEvent();
                }
            }
        }
    });

    const sideBarNavigationRootObserver = new MutationObserver((mutationsList) => {
        for (const mutation of mutationsList) {
            if (mutation.type === 'childList') {
                const navLinks = mutation.target.querySelectorAll('.nav-list__link a');
                if (navLinks && navLinks.length > 0) {
                    ScheduledJobExtra.registerNavigationLinkClickEvent(navLinks, rootContentObserver, rootDiv, observerConfig);
                    sideBarNavigationRootObserver.disconnect();
                }
            }
        }
    });

    const scheduledJobForm = rootDiv.querySelector('.scheduled-job');
    const scheduledJobList = rootDiv.querySelector('.scheduled-jobs-list');

    if (scheduledJobForm || scheduledJobList) {
        rootContentObserver.observe(rootDiv, observerConfig);
    }

    sideBarNavigationRootObserver.observe(sideBarNavigationRoot, observerConfig);

    const navLinks = sideBarNavigationRoot.querySelectorAll('.nav-list__link a');
    if (navLinks && navLinks.length > 0) {
        ScheduledJobExtra.registerNavigationLinkClickEvent(navLinks, rootContentObserver, rootDiv, observerConfig);
        sideBarNavigationRootObserver.disconnect();
    }    
});

const ScheduledJobExtra = {
    registerNavigationLinkClickEvent: (navLinks, observer, rootDiv, config) => {
        navLinks.forEach(x => {
            x.addEventListener("click", () => {
                const href = x.getAttribute("href") || '';
                if (href.indexOf('#/ScheduledJobs') !== -1) {
                    observer.observe(rootDiv, config);
                }
                else {
                    observer.disconnect();
                }
            });
        });
    },
    showNotification: (message, type) => {
        const isError = type === 'error';

        // Toast styles live in ClientResources/Styles/ScheduledJobExtraParameters.css
        // (registered as an admin Style resource in module.config).
        let container = document.querySelector('.extra-params-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'extra-params-toast-container';
            document.body.appendChild(container);
        }

        const successIcon = `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#2e7d32" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><path d="M8 12l3 3 5-6"></path></svg>`;
        const errorIcon = `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#c62828" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><path d="M15 9l-6 6M9 9l6 6"></path></svg>`;

        const toast = document.createElement('div');
        toast.className = 'extra-params-toast' + (isError ? ' error' : '');
        toast.innerHTML = `<span class="extra-params-toast__icon">${isError ? errorIcon : successIcon}</span><span class="extra-params-toast__msg"></span><button type="button" class="extra-params-toast__close" aria-label="Close">&times;</button>`;
        // Use textContent for the message so error text can't inject markup.
        toast.querySelector('.extra-params-toast__msg').textContent = message;

        const remove = () => { if (toast.parentNode) { toast.parentNode.removeChild(toast); } };
        toast.querySelector('.extra-params-toast__close').addEventListener('click', remove);
        container.appendChild(toast);
        setTimeout(remove, 4000);
    },
    saveExtraParams: (extraParamsForm, scheduledJobId) => {
        // Serialize the form data
        const formData = new FormData(extraParamsForm);
        const saveExtraParamsApi = EXTRA_PARAMS_API_BASE + '/save?scheduledJobId=' + scheduledJobId;
        // Submit the form data asynchronously using fetch API
        fetch(saveExtraParamsApi, {
            method: 'POST',
            body: formData
        }).then(response => {
            if (!response.ok) {
                throw new Error('Extra parameters saved failed');
            }
            ScheduledJobExtra.showNotification('Extra parameters have been saved.', 'success');
        }).catch(error => {
            ScheduledJobExtra.showNotification(error && error.message ? error.message : 'Extra parameters save failed.', 'error');
        });
    },
    renderExtraParametersView: (scheduledJobId) => {
        const getExtraParametersViewApi = EXTRA_PARAMS_API_BASE + '/getView?scheduledJobId=' + scheduledJobId;
        fetch(getExtraParametersViewApi, {
            method: 'GET'
        }).then(response => {
            if (!response.ok) {
                throw new Error('Getting extra params view failed');
            }
            if (response.status == 204) {
                return null;
            }
            return response.text();
        }).then(data => {
            if (data) {
                let scheduledJobForm = document.getElementById("root").querySelector('.scheduled-job');

                if (scheduledJobForm.querySelector('.extraParams')) {
                    return;
                }

                let extraParamsContainer = document.createElement('div');
                extraParamsContainer.classList.add('extraParams');
                extraParamsContainer.innerHTML = data;

                scheduledJobForm.appendChild(extraParamsContainer);

                let extraParamsForm = scheduledJobForm.querySelector('#extra-params-form');

                if (extraParamsForm) {
                    // Add a submit event listener to the form
                    extraParamsForm.addEventListener('submit', function (event) {
                        // Prevent the default form submission behavior
                        event.preventDefault();
                        ScheduledJobExtra.saveExtraParams(extraParamsForm, scheduledJobId);
                    });
                }

            }
        }).catch(error => {
            console.log(error);
        });
    },
    addScheduledJobSettingsClickEvent: () => {
        const currentUrlSegments = window.location.href.split('/');
        const scheduledJobId = currentUrlSegments[currentUrlSegments.length - 1]

        if (scheduledJobId) {
            const hasSupportForExtraParametersViewApi = EXTRA_PARAMS_API_BASE + '/hasSupportForExtraParameters?scheduledJobId=' + scheduledJobId;
            fetch(hasSupportForExtraParametersViewApi, {
                method: 'GET'
            }).then(response => {
                if (!response.ok) {
                    throw new Error('Getting extra params view failed');
                }
                if (response.status == 204) {
                    return null;
                }
                return response.json();
            }).then(data => {
                if (data) {                    
                    ScheduledJobExtra.renderExtraParametersView(scheduledJobId);                    
                }
            }).catch(error => {
                console.log(error);
            });
        }
    }
}