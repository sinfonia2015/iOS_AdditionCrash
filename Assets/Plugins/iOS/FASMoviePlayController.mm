#import "FASMoviePlayController.h"
#import "AVPlayerViewController.h"

extern UIViewController *UnityGetGLViewController();

extern UIView *UnityGetGLView();

char strUrl[512];

NSURL *url;

NSString *callbackGameObjectName;

bool like;

int likeCount;

int playbackCount;

NSString *facebookText;

NSString *facebookUrl;

NSString *twitterText;

NSString *twitterUrl;

NSString *closeText;

NSString *userImagePath;

NSString *appIconPath;

NSString *appName;

NSMutableArray *shareUrls;

NSString *shareUrl0;

NSString *shareUrl1;

NSString *errorText;

int urlNum;

bool userButtonEnabled;

@implementation FASMoviePlayController

static FASMoviePlayController *pInstance = nil;

+ (FASMoviePlayController*) instance {
    @synchronized(self) {
        if(pInstance == nil) {
            pInstance = [[self alloc]init];
        }
    }
    return pInstance;
}

- (void)playMovie {
    
    AVPlayerViewController *vc = [AVPlayerViewController controller:url];
    
    vc.like = like;
    
    vc.likeCount = likeCount;
    
    vc.playbackCount = playbackCount;
    
    vc.facebookText = facebookText;
    
    vc.facebookUrl = facebookUrl;
    
    vc.twitterText = twitterText;
    
    vc.twitterUrl = twitterUrl;
    
    vc.callbackGameObjectName = callbackGameObjectName;
    
    vc.closeText = closeText;
    
    vc.userImagePath = userImagePath;
    
    vc.appIconPath = appIconPath;
    
    vc.appName = appName;
    
    vc.userButtonEnabled = userButtonEnabled;
    
    vc.shareUrls = shareUrls;
    
    vc.urlNum = urlNum;
    
    vc.shareUrl0 = shareUrl0;
    
    vc.shareUrl1 = shareUrl1;
    
    vc.errorText = errorText;
    
    [UnityGetGLViewController() presentViewController:vc animated:YES completion:nil];
    
    [vc autorelease];
    
    vc = nil;
}

@end

extern "C"
{
    void _FasMoviePlay(const char *url)
    {
        strncpy(strUrl, url, sizeof(strUrl));
        
        [[FASMoviePlayController instance] playMovie];
    }

    void _FasMoviePlayWithParameters(const char *_url, const char *_callbackGameObjectName, bool _like, int _likeCount, int _playbackCount, const char *_facebookText, const char *_facebookUrl, const char *_twitterText, const char *_twitterUrl, const char *_closeText, const char *_userImagePath, const char *_appIconPath, const char *_appName, bool _userButtonEnabled)
    {
        strncpy(strUrl, _url, sizeof(strUrl));
        
        NSString *str = [[NSString alloc] initWithUTF8String:strUrl];
        
        url = [NSURL URLWithString:str];
        
        callbackGameObjectName =[[NSString alloc] initWithUTF8String:_callbackGameObjectName];
        
        like = _like;
        
        likeCount = _likeCount;
        
        playbackCount = _playbackCount;
        
        facebookText =[[NSString alloc] initWithUTF8String:_facebookText];
        
        facebookUrl =[[NSString alloc] initWithUTF8String:_facebookUrl];
        
        twitterText =[[NSString alloc] initWithUTF8String:_twitterText];

        twitterUrl =[[NSString alloc] initWithUTF8String:_twitterUrl];

        closeText =[[NSString alloc] initWithUTF8String:_closeText];
        
        userImagePath =[[NSString alloc] initWithUTF8String:_userImagePath];
        
        appIconPath =[[NSString alloc] initWithUTF8String:_appIconPath];
        
        appName = [[NSString alloc] initWithUTF8String:_appName];
        
        userButtonEnabled = _userButtonEnabled;
        
        shareUrls = NULL;
        
        urlNum = 0;
        
        [[FASMoviePlayController instance] playMovie];
    }
    
    void _FasMoviePlayWithUrls(const char *_url, const char *_callbackGameObjectName, bool _like, int _likeCount, int _playbackCount, const char *_text, const char **_urls, int _urlNum, const char *_closeText, const char *_userImagePath, const char *_appIconPath, const char *_appName, bool _userButtonEnabled, const char *_errorText)
    {
        strncpy(strUrl, _url, sizeof(strUrl));
        
        NSString *str = [[NSString alloc] initWithUTF8String:strUrl];
        
        url = [NSURL URLWithString:str];
        
        callbackGameObjectName =[[NSString alloc] initWithUTF8String:_callbackGameObjectName];
        
        like = _like;
        
        likeCount = _likeCount;
        
        playbackCount = _playbackCount;
        
        facebookText =[[NSString alloc] initWithUTF8String:_text];
        
        facebookUrl = NULL;
        
        twitterUrl = NULL;
        
        twitterText =[[NSString alloc] initWithUTF8String:_text];
        
        shareUrls =  [NSMutableArray arrayWithCapacity:_urlNum];
        
        errorText =[[NSString alloc] initWithUTF8String:_errorText];

        urlNum = _urlNum;

        if(urlNum > 0)
            shareUrl0 =[[NSString alloc] initWithUTF8String:_urls[0]];

        if(urlNum > 1)
            shareUrl1 =[[NSString alloc] initWithUTF8String:_urls[1]];

        for(int i = 0; i < _urlNum; i++)
        {
            NSString *str =[[NSString alloc] initWithUTF8String:_urls[i]];
            
            [shareUrls addObject:str];
        }
        
        closeText =[[NSString alloc] initWithUTF8String:_closeText];
        
        userImagePath =[[NSString alloc] initWithUTF8String:_userImagePath];
        
        appIconPath =[[NSString alloc] initWithUTF8String:_appIconPath];
        
        appName = [[NSString alloc] initWithUTF8String:_appName];
        
        userButtonEnabled = _userButtonEnabled;
        
        [[FASMoviePlayController instance] playMovie];
    }

}