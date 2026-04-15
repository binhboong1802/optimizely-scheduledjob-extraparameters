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
                if (x.getAttribute("href") == '/EPiServer/EPiServer.Cms.UI.Admin/default#/ScheduledJobs') {
                    observer.observe(rootDiv, config);
                }
                else {
                    observer.disconnect();
                }
            });
        });
    },
    saveExtraParams: (extraParamsForm, scheduledJobId) => {
        // Serialize the form data
        const formData = new FormData(extraParamsForm);
        const saveExtraParamsApi = '/episerver/Optimizely.ScheduledJob.ExtraParameters/ScheduledJobExtraParameters/save?scheduledJobId=' + scheduledJobId;
        // Submit the form data asynchronously using fetch API
        fetch(saveExtraParamsApi, {
            method: 'POST',
            body: formData
        }).then(response => {
            if (!response.ok) {
                throw new Error('Extra parameters saved failed');
            }
            alert('Extra parameters saved successfully');
            window.location.href = `/EPiServer/EPiServer.Cms.UI.Admin/default#/ScheduledJobs/detailScheduledJob/${scheduledJobId}`;
        }).catch(error => {
            alert(error);
        });
    },
    renderExtraParametersView: (scheduledJobId) => {
        const getExtraParametersViewApi = '/episerver/Optimizely.ScheduledJob.ExtraParameters/ScheduledJobExtraParameters/getView?scheduledJobId=' + scheduledJobId;
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
            const hasSupportForExtraParametersViewApi = '/episerver/Optimizely.ScheduledJob.ExtraParameters/ScheduledJobExtraParameters/hasSupportForExtraParameters?scheduledJobId=' + scheduledJobId;
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