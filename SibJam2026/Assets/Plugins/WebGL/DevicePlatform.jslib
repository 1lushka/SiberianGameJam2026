mergeInto(LibraryManager.library, {
    DevicePlatform_IsMobileBrowser: function () {
        var n = navigator;
        return (n.userAgentData && n.userAgentData.mobile) ||
            /Android|iPhone|iPad|iPod/i.test(n.userAgent) ||
            (n.platform === 'MacIntel' && n.maxTouchPoints > 1) ? 1 : 0;
    }
});
