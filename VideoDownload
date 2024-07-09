// ==UserScript==
// @name         Video Downloader
// @namespace    http://tampermonkey.net/
// @version      0.1
// @description  Try to download video from the current page
// @author       Your Name
// @match        *://*/*
// @grant        none
// ==/UserScript==
//此脚本会在页面上查找
//<video>
//元素，提取其源 URL，并在页面上生成一个下载链接。请注意，这只是一个基本示例，具体网站的结构可能有所不同，因此需要根据具体情况进行调整。

//再次提醒，务必确认你下载视频的行为是合法的，并且遵守相关网站的服务条款。



(function() {
    'use strict';

    // This script example looks for a video tag on the page
    let video = document.querySelector('video');
    
    if (video) {
        let videoSrc = video.getAttribute('src');
        
        if (!videoSrc) {
            // For some sites, the video source might be within a <source> tag
            let source = video.querySelector('source');
            if (source) {
                videoSrc = source.getAttribute('src');
            }
        }
        
        if (videoSrc) {
            let a = document.createElement('a');
            a.href = videoSrc;
            a.download = 'video';
            a.textContent = 'Download Video';
            document.body.appendChild(a);
        } else {
            console.log('No video source found.');
        }
    } else {
        console.log('No video element found.');
    }
})();
