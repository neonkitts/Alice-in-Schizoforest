mergeInto(LibraryManager.library, {
  AliceTouchState: function(playing,cat){if(window.aliceTouchState)window.aliceTouchState(playing,cat);},
  AliceIsMobileDevice: function(){
    var ua=navigator.userAgent||'';
    var hinted=!!(navigator.userAgentData&&navigator.userAgentData.mobile);
    var ipad=navigator.platform==='MacIntel'&&navigator.maxTouchPoints>1;
    return hinted||ipad||/Android|iPhone|iPad|iPod|IEMobile|Opera Mini|Mobile/i.test(ua)?1:0;
  },
  AliceReady: function(){window.aliceReady=true;if(window.ysdk)window.ysdk.features.LoadingAPI.ready();},
  AliceGameplay: function(playing){window.alicePlaying=!!playing;if(window.ysdk){var api=window.ysdk.features.GameplayAPI;if(playing)api.start();else api.stop();}}
});
