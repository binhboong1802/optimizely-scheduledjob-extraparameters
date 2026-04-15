document.addEventListener('DOMContentLoaded', function () {
    const rootDiv = document.getElementById('root');
    const sideBarNavigationRoot = document.getElementById('sideBarNavigationRoot');
    const observerConfig = { childList: true, subtree: true };

    const rootContentObserver = new MutationObserver((mutationsList, observer) => {
        for (const mutation of mutationsList) {
            if (mutation.type === 'childList') {
                const scheduledJobNav = mutation.target.querySelector('.scheduled-job-nav .axiom-side-nav__list');
                if (scheduledJobNav) {
                    ScheduledJobExtra.renderScheduledJobExtraParametersTab(scheduledJobNav, rootDiv);
                }
            }
        }
    });

    const sideBarNavigationRootObserver = new MutationObserver((mutationsList, observer) => {
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
        const saveExtraParamsApi = '/episerver/admin/scheduledjob/extraparams/save?scheduledJobId=' + scheduledJobId;
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
        const getExtraParametersViewApi = '/episerver/admin/scheduledjob/extraparams/getView?scheduledJobId=' + scheduledJobId;
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

                let extraParamsContainer = document.createElement('div');
                extraParamsContainer.innerHTML = `<span class="axiom-typography--header1">Extra Parameters</span>
                                                <div class="scheduled-job--body">
                                                <div class="extra-params-form-container">
                                                    <form id="extra-params-form">
                                                        ${data}
                                                        <div class="btn-save-extra-params" style="display: block;float:right;margin-top:20px;">
                                                        <button data-oui-component="true" class="oui-button oui-button--highlight oui-button--default" type="submit" aria-live="polite">Save</button>
                                                        </div>
                                                    </form>
                                                </div>
                                                </div>`;

                scheduledJobForm.innerHTML = extraParamsContainer.innerHTML;

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
    renderScheduledJobExtraParametersTab: (scheduledJobNav, rootDiv) => {
        let liExtraParams = scheduledJobNav.querySelector('.li-extra-params');
        const currentUrlSegments = window.location.href.split('/');
        const scheduledJobId = currentUrlSegments[currentUrlSegments.length - 1]

        if (!liExtraParams) {
            const hasSupportForExtraParametersViewApi = '/episerver/admin/scheduledjob/extraparams/hasSupportForExtraParameters?scheduledJobId=' + scheduledJobId;
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
                if (data && scheduledJobNav.querySelectorAll('.li-extra-params').length == 0) {
                    liExtraParams = document.createElement('li');
                    liExtraParams.classList.add('nav-list__link');
                    liExtraParams.classList.add('li-extra-params');
                    liExtraParams.innerHTML = '<div class="flex flex-justified--between flex-align--center flex--1 height--1-1"><a data-oui-component="true" class="link width--1-1 display--inline-block " tabindex="0">Extra Parameters</a></div>';
                    scheduledJobNav.appendChild(liExtraParams);

                    scheduledJobNav.querySelectorAll('.nav-list__link').forEach(item => {
                        item.addEventListener('click', () => {
                            if (!item.classList.contains('li-extra-params')) {
                                liExtraParams.classList.remove('is-active');
                                return;
                            }
                            const activeNav = scheduledJobNav.querySelector('.is-active');
                            if (activeNav) {
                                activeNav.classList.remove('is-active');
                            }
                            item.classList.add('is-active');

                            ScheduledJobExtra.renderExtraParametersView(scheduledJobId);
                        });
                    });
                }
            }).catch(error => {
                console.log(error);
            });
        }
    }
}