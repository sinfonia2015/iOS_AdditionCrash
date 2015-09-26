#import "AVPlayerViewController.h"
#import <AVFoundation/AVFoundation.h>
#import "FASShareTwitter.h"
#import "FASShareFacebook.h"
#import <Social/Social.h>
#import <Accounts/Accounts.h>
#import <QuartzCore/QuartzCore.h>

extern UIViewController *UnityGetGLViewController();

NSString* const kStatusKey = @"status";
static void* AVPlayerViewControllerStatusObservationContext = &AVPlayerViewControllerStatusObservationContext;

@interface AVPlayerViewController ()

@property (retain, nonatomic) IBOutlet UIView *topUIView;
@property (retain, nonatomic) IBOutlet AVPlayerView *playerView;
@property (retain, nonatomic) IBOutlet UIView *bottomUIView;
@property (retain, nonatomic) IBOutlet UIButton *buttonLikeOff;
@property (retain, nonatomic) IBOutlet UIButton *buttonLikeOn;
@property (retain, nonatomic) IBOutlet UILabel *likeCountText;
@property (retain, nonatomic) IBOutlet UILabel *playbackCountText;
@property (retain, nonatomic) IBOutlet UIActivityIndicatorView *activityIndicator;
@property (retain, nonatomic) IBOutlet UIButton *buttonClose;
@property (retain, nonatomic) IBOutlet UIButton *buttonUser;
@property (retain, nonatomic) IBOutlet UIButton *buttonApp;
@property (retain, nonatomic) IBOutlet UILabel *labelAppName;
@property (retain, nonatomic) IBOutlet UIButton *buttonLargePlay;

@property (nonatomic, retain) NSURL*        videoUrl;
@property (nonatomic, retain) AVPlayerItem* playerItem;
@property (nonatomic, retain) AVPlayer*     videoPlayer;
@property (strong, nonatomic) AVPlayerLayer *playerLayer;
@property (nonatomic, assign) id            playTimeObserver;
@property (nonatomic, assign) BOOL          isPlaying;
@property (nonatomic, assign) BOOL          uiHidden;
@property (nonatomic, assign) BOOL          initLike;
@property (nonatomic, assign) BOOL          closed;
@property (nonatomic, assign) BOOL          videoLoaded;
@property (nonatomic, assign) BOOL          closeEnable;

@end

@implementation AVPlayerViewController

#pragma mark - Lifecycle

+ (AVPlayerViewController *)controller:(NSURL *)videoUrl
{
    AVPlayerViewController* controller = [[[AVPlayerViewController alloc] initWithNibName:@"AVPlayerViewController" bundle:nil] autorelease];
    controller.videoUrl = videoUrl;

    return controller;
}

- (void)dealloc
{
    [self.videoPlayer pause];
    [self.playerItem removeObserver:self forKeyPath:kStatusKey context:AVPlayerViewControllerStatusObservationContext];
    [[NSNotificationCenter defaultCenter] removeObserver:self name:AVPlayerItemDidPlayToEndTimeNotification object:self.playerItem];

    self.videoPlayerView  = nil;
    self.videoPlayer      = nil;
    self.videoUrl         = nil;
    self.playerItem       = nil;
    self.currentTimeLabel = nil;
    self.seekBar          = nil;
    self.durationLabel    = nil;
    self.playButton       = nil;
    self.playerToolView   = nil;

    [_playerView release];
    [_topUIView release];
    [_bottomUIView release];
    [_buttonLikeOff release];
    [_buttonLikeOn release];
    [_likeCountText release];
    [_playbackCountText release];
    [_activityIndicator release];
    [_buttonClose release];
    [_buttonUser release];
    [_buttonApp release];
    [_labelAppName release];
    [_buttonLargePlay release];
    [super dealloc];
}

#pragma mark - View

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    self.title = @"AVPlayer";
    
    NSLog(@"%@", self.shareUrl0);
    NSLog(@"%@", self.shareUrl1);
    
    self.initLike = self.like;
    
    self.closeEnable = false;

    self.buttonClose.titleLabel.font   = [UIFont fontWithName:@"Koruri-Light" size:16.0f];
    
    [self.likeCountText setText:[NSString stringWithFormat:@"%d", self.likeCount]];
    
    self.likeCountText.numberOfLines = 0;
    
    [self.likeCountText sizeToFit];
    
    [self.playbackCountText setText:[NSString stringWithFormat:@"%d", self.playbackCount]];

    self.playbackCountText.numberOfLines = 0;
    
    [self.playbackCountText sizeToFit];
    
    if(self.appIconPath && self.appIconPath.length){
        
        UIImage *appIcon = [UIImage imageWithContentsOfFile:self.appIconPath];

        [self.buttonApp setImage:appIcon forState:normal];
        
        self.buttonApp.layer.cornerRadius = 5;
        
        self.buttonApp.layer.masksToBounds = YES;

        [self.labelAppName setText:self.appName];
        
        self.labelAppName.hidden = NO;
    }
    else{
     
        self.buttonApp.hidden = YES;

        self.labelAppName.hidden = YES;
    }
    
    self.labelAppName.font = [UIFont fontWithName:@"Koruri-Light" size:18.0f];
    
    if(self.userImagePath && self.userImagePath.length){

        UIImage *userImage = [UIImage imageWithContentsOfFile:self.userImagePath];
        
        [self.buttonUser setImage:userImage forState:normal];
        
        self.buttonUser.layer.cornerRadius = 15;
        
        self.buttonUser.layer.masksToBounds = YES;
    }
    
    [self.playButton addTarget:self action:@selector(play:) forControlEvents:UIControlEventTouchDown];
    
    self.playButton.contentMode = UIViewContentModeCenter;
    
    self.playButton.enabled = NO;
    
    self.seekBar.enabled    = NO;
    
    UIImage *sliderThumb = [[UIImage imageNamed:@"video_player_thumb.png"] stretchableImageWithLeftCapWidth: 7 topCapHeight: 0];
    
    [self.seekBar setThumbImage:sliderThumb forState:UIControlStateNormal];
    
    self.playerItem = [[[AVPlayerItem alloc] initWithURL:self.videoUrl] autorelease];
    
    [self.playerItem addObserver:self
                      forKeyPath:kStatusKey
                         options:NSKeyValueObservingOptionInitial | NSKeyValueObservingOptionNew
                         context:AVPlayerViewControllerStatusObservationContext];

  	// end notification
    [[NSNotificationCenter defaultCenter] addObserver:self
											 selector:@selector(playerDidPlayToEndTime:)
												 name:AVPlayerItemDidPlayToEndTimeNotification
											   object:self.playerItem];
  
    self.videoPlayer = [[[AVPlayer alloc] initWithPlayerItem:self.playerItem] autorelease];
        
    self.playerLayer = [AVPlayerLayer playerLayerWithPlayer:self.videoPlayer];
    
    [self.videoPlayerView.layer addSublayer:self.playerLayer];

    UIInterfaceOrientation orientation = [UIApplication sharedApplication].statusBarOrientation;
    
    CGRect frame;
    
    switch (orientation)
    {
        case UIInterfaceOrientationLandscapeLeft:
            
        case UIInterfaceOrientationLandscapeRight:
            
            frame = CGRectMake(0.0, 0.0, UnityGetGLViewController().view.bounds.size.height, _playerView.bounds.size.width);
            
            break;
            
        default:
            
            frame = CGRectMake(0.0, 0.0, UnityGetGLViewController().view.bounds.size.width, _playerView.bounds.size.height);
            
            break;
    }
    
    self.playerLayer.frame = frame;
    
    self.playerLayer.videoGravity = AVLayerVideoGravityResizeAspect;
    
	UITapGestureRecognizer* tapSingle = [[[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(tapSingle:)] autorelease];
	
    tapSingle.numberOfTapsRequired = 1;
	
    [self.videoPlayerView addGestureRecognizer:tapSingle];
    
    [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
    
    [[NSNotificationCenter defaultCenter]
     
     addObserver:self selector:@selector(orientationChanged:)
     
     name:UIDeviceOrientationDidChangeNotification
    
     object:[UIDevice currentDevice]];
    
    [_buttonLikeOn setHidden:!self.like];
    
    [_buttonLikeOff setHidden:self.like];

    [_activityIndicator startAnimating];
}

- (void) orientationChanged:(NSNotification *)note
{
    [self setDeviceOrientation];
}

- (void)setDeviceOrientation
{
    CGRect frame = _playerView.bounds;

    self.playerLayer.frame = frame;
    
    self.playerLayer.videoGravity = AVLayerVideoGravityResizeAspect;
}

- (BOOL)prefersStatusBarHidden
{
    return YES;
}

- (void)applicationDidEnterBackground:(UIApplication *)application
{
    if( self.isPlaying )
    {
        self.isPlaying = NO;
        
        [self.videoPlayer pause];
    }
    
    [self syncPlayButton];
}

- (void)viewDidUnload
{
    [self.videoPlayer pause];

    [self.playerItem removeObserver:self forKeyPath:kStatusKey context:AVPlayerViewControllerStatusObservationContext];
    
    [[NSNotificationCenter defaultCenter] removeObserver:self name:AVPlayerItemDidPlayToEndTimeNotification object:self.playerItem];
    
    self.videoPlayerView  = nil;
    self.videoPlayer      = nil;
    self.videoUrl         = nil;
    self.playerItem       = nil;
    self.currentTimeLabel = nil;
    self.seekBar          = nil;
    self.durationLabel    = nil;
    self.playButton       = nil;
    self.playerToolView   = nil;

    [super viewDidUnload];
}

- (void)viewWillAppear:(BOOL)animated
{
    [super viewWillAppear:animated];

    self.buttonLargePlay.hidden = YES;

    [self.navigationController.navigationBar setBarStyle:UIBarStyleBlackTranslucent];
    [self.navigationController.navigationBar setTranslucent:YES];
    [self.navigationController.view setNeedsLayout];
}


- (void)viewWillDisappear:(BOOL)animated
{
    [super viewWillDisappear:animated];
}

- (void)viewDidDisappear:(BOOL)animated
{
    [super viewDidDisappear:animated];
    
    self.view = nil;
}

-(void)viewDidAppear:(BOOL)animated
{
    [self performSelector:@selector(showCloseButton) withObject:nil afterDelay:1];
}

- (void)showCloseButton
{
    self.closeEnable = true;
}

- (void)observeValueForKeyPath:(NSString *)keyPath ofObject:(id)object change:(NSDictionary *)change context:(void *)context
{
    if(self == NULL || self.closed){
        return;
    }
    
    if( context == AVPlayerViewControllerStatusObservationContext )
    {
        const AVPlayerStatus status = [[change objectForKey:NSKeyValueChangeNewKey] integerValue];
        
        switch( status )
        {

        case AVPlayerStatusReadyToPlay:
                
                self.videoLoaded = YES;
                
                [self setupSeekBar];
                
                self.playButton.enabled = YES;
                
                self.seekBar.enabled    = YES;

                [_activityIndicator stopAnimating];

                [_activityIndicator setHidden:YES];
                
                CGRect frame = _playerView.bounds;
                
                self.playerLayer.frame = frame;
                
                self.playerLayer.videoGravity = AVLayerVideoGravityResizeAspect;
                
                [self playVideo];
            break;

        case AVPlayerStatusUnknown:
                
                self.closeEnable = true;
                
                [self showError:nil];
            break;

        case AVPlayerStatusFailed:
            {
                AVPlayerItem* playerItem = ( AVPlayerItem* )object;
                
                self.closeEnable = true;
                
                [self showError:playerItem.error];
            }
            break;
        }
    }
}

#pragma mark - Private

- (void)play:(id)sender
{
    [self playVideo];
}

- (void)playVideo
{
    if(CMTimeGetSeconds(self.playerItem.currentTime) == CMTimeGetSeconds(self.playerItem.duration))
    {
        [self.videoPlayer seekToTime:kCMTimeZero];
    }

    if( self.isPlaying )
    {
        self.isPlaying = NO;
        
        [self.videoPlayer pause];
    }
    else
    {
        self.isPlaying = YES;
        
        [self.videoPlayer play];
    }
    
    [self syncPlayButton];
    
    [self performSelector:@selector(hideUI) withObject:nil afterDelay:3];
}

- (void)hideUI
{
    if(!self.uiHidden)
        [self setUI:YES];
}

- (void)playerDidPlayToEndTime:(NSNotification *)notification
{
    self.isPlaying = NO;
    
    [self syncPlayButton];

    if(self.uiHidden)
    {
        [self setUI:false];
    }
}


- (void)removePlayerTimeObserver
{
    if( self.playTimeObserver == nil ) { return; }

    [self.videoPlayer removeTimeObserver:self.playTimeObserver];
    
    self.playTimeObserver = nil;
}

- (void)seekBarValueChanged:(UISlider *)slider
{
	[self.videoPlayer seekToTime:CMTimeMakeWithSeconds( slider.value, NSEC_PER_SEC )];
}


- (void)setupSeekBar
{
	self.seekBar.minimumValue = 0;
	self.seekBar.maximumValue = CMTimeGetSeconds( self.playerItem.duration );
	self.seekBar.value        = 0;
	[self.seekBar addTarget:self action:@selector(seekBarValueChanged:) forControlEvents:UIControlEventValueChanged];
    

    const double interval = ( 0.5f * self.seekBar.maximumValue ) / self.seekBar.bounds.size.width;
	const CMTime time     = CMTimeMakeWithSeconds( interval, NSEC_PER_SEC );
	self.playTimeObserver = [self.videoPlayer addPeriodicTimeObserverForInterval:time
                                                                           queue:NULL
                                                                      usingBlock:^( CMTime time ) { [self syncSeekBar]; }];

    self.durationLabel.text = [self timeToString:self.seekBar.maximumValue];
}


- (void)showError:(NSError *)error
{
    [self removePlayerTimeObserver];
    [self syncSeekBar];
    self.playButton.enabled = NO;
    self.seekBar.enabled    = NO;
    
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:self.errorText message:@"" preferredStyle:UIAlertControllerStyleAlert];
        
    [alertController addAction:[UIAlertAction actionWithTitle:self.closeText style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
        [self close];
    }]];
        
    [self presentViewController:alertController animated:YES completion:nil];
}


- (void)syncSeekBar
{
	const double duration = CMTimeGetSeconds( [self.videoPlayer.currentItem duration] );
	const double time     = CMTimeGetSeconds([self.videoPlayer currentTime]);
	const float  value    = ( self.seekBar.maximumValue - self.seekBar.minimumValue ) * time / duration + self.seekBar.minimumValue;
    
	[self.seekBar setValue:value];
    self.currentTimeLabel.text = [self timeToString:self.seekBar.value];
}


- (void)syncPlayButton
{
    if( self.isPlaying )
    {
        [self.playButton setImage:[UIImage imageNamed:@"pause"] forState:UIControlStateNormal];
        
        if(!self.buttonLargePlay.hidden){
            
            [UIView animateWithDuration:0.3 animations:^{
                
                self.buttonLargePlay.alpha = 0;
                
            } completion:^(BOOL finished){

                self.buttonLargePlay.hidden = finished;
    
            }];
        }
    }
    else
    {
        [self.playButton setImage:[UIImage imageNamed:@"play"] forState:UIControlStateNormal];
        
        if(self.buttonLargePlay.hidden){
            
            self.buttonLargePlay.alpha = 0;
            
            self.buttonLargePlay.hidden = NO;
            
            [UIView animateWithDuration:0.3 animations:^{
                
                self.buttonLargePlay.alpha = 1;
                
            } completion:^(BOOL finished){
                
            }];
        }
    }
}


- (void)tapSingle:(UITapGestureRecognizer *)sender
{
    [self toggleUI];
}

- (void) toggleUI
{
    self.uiHidden = !self.uiHidden;
    
    [self setUI:self.uiHidden];
}

- (void) setUI:(BOOL) hidden
{
    self.uiHidden = hidden;
    
    if(hidden){
        
        [UIView animateWithDuration:0.3 animations:^{
            
            self.topUIView.alpha = 0;
            
            self.bottomUIView.alpha = 0;
            
        } completion:^(BOOL finished){
            
            self.topUIView.hidden = finished;
            
            self.bottomUIView.hidden = finished;
        }];
        
    }
    else{
        
        self.topUIView.alpha = 0;
        
        self.bottomUIView.alpha = 0;
        
        self.topUIView.hidden = NO;
        
        self.bottomUIView.hidden = NO;
        
        [UIView animateWithDuration:0.3 animations:^{
            
            self.topUIView.alpha = 1;
            
            self.bottomUIView.alpha = 1;
            
        } completion:^(BOOL finished){
            
        }];
    }

}


- (NSString* )timeToString:(float)value
{
    const NSInteger time = value;
    
    return [NSString stringWithFormat:@"%02d:%02d", ( int )( time / 60 ), ( int )( time % 60 )];
}

- (IBAction) pushedCloseButton:(id) sender{
    
    [self close];

}

- (void) close {

    if(!self.closeEnable) return;
    
    [self.playerItem  removeObserver:self forKeyPath:kStatusKey];
    
    self.playerItem = nil;
    
    _closed = YES;
    
    self.isPlaying = NO;
    
    [self.videoPlayer pause];
    
    if(self.like != self.initLike)
    {
        UnitySendMessage([self.callbackGameObjectName UTF8String], "OnLikeStateChanged", "");
    }
    
    UnitySendMessage([self.callbackGameObjectName UTF8String], "OnFinish", "");
    
    [self dismissViewControllerAnimated:YES completion:^ {}];
    
    [self.playerItem removeObserver:self forKeyPath:kStatusKey context:AVPlayerViewControllerStatusObservationContext];
    
    [[NSNotificationCenter defaultCenter] removeObserver:self name:AVPlayerItemDidPlayToEndTimeNotification object:self.playerItem];
    
    [self.videoPlayer  replaceCurrentItemWithPlayerItem:(nil)];
    
    [self.playerLayer removeFromSuperlayer];
    
    self.videoPlayer = nil;
    
}

- (IBAction) pushedLikeButton:(id) sender{
    
    self.like = YES;
    
    self.likeCount++;
    
    [self.likeCountText setText:[NSString stringWithFormat:@"%d", self.likeCount]];

    [_buttonLikeOn setHidden:!self.like];

    [_buttonLikeOff setHidden:self.like];

}


- (IBAction) pushedUnlikeButton:(id) sender{
    
    self.like = NO;
    
    self.likeCount--;
    
    if(self.likeCount < 0)
        self.likeCount = 0;
    
    [self.likeCountText setText:[NSString stringWithFormat:@"%d", self.likeCount]];

    [_buttonLikeOn setHidden:!self.like];
    
    [_buttonLikeOff setHidden:self.like];
    
}

- (IBAction) pushedTwitterButton:(id) sender{

    if ([SLComposeViewController isAvailableForServiceType:SLServiceTypeTwitter])
    {
        SLComposeViewController *composeVC = [SLComposeViewController composeViewControllerForServiceType:SLServiceTypeTwitter];
        
        [composeVC setCompletionHandler:^(SLComposeViewControllerResult result) {
            dispatch_async(dispatch_get_main_queue(), ^
                           {
                               switch(result)
                               {
                                   case SLComposeViewControllerResultCancelled:
                                   default:

                                       break;
                                       
                                   case SLComposeViewControllerResultDone:
                                       
                                       break;
                               }
                           });
        }];
        
        [composeVC setInitialText:self.twitterText];

        if(self.shareUrls == NULL)
        {
            [composeVC addURL:[NSURL URLWithString:self.twitterUrl]];
        }
        else
        {
            if(self.urlNum > 0)
            {
                [composeVC addURL:[NSURL URLWithString:self.shareUrl0]];
            }
            if(self.urlNum > 1)
            {
                [composeVC addURL:[NSURL URLWithString:self.shareUrl1]];
            }
        }
        
        [self presentViewController:composeVC animated:YES completion:nil];
    }
}

- (IBAction) pushedFacebookButton:(id) sender{
    
    if ([SLComposeViewController isAvailableForServiceType:SLServiceTypeFacebook])
    {
        SLComposeViewController *composeViewController = [SLComposeViewController composeViewControllerForServiceType:SLServiceTypeFacebook];

        [composeViewController setInitialText:self.facebookText];
        
        if(self.shareUrls == NULL)
        {
            [composeViewController addURL:[NSURL URLWithString:self.twitterUrl]];
        }
        else
        {
            if(self.urlNum > 0)
            {
                [composeViewController addURL:[NSURL URLWithString:self.shareUrl0]];
            }

            if(self.urlNum > 1)
            {
                [composeViewController addURL:[NSURL URLWithString:self.shareUrl1]];
            }
        }
        
        [composeViewController setCompletionHandler:^(SLComposeViewControllerResult result)
         {
             dispatch_async(dispatch_get_main_queue(), ^
                            {
                                switch(result)
                                {
                                    case SLComposeViewControllerResultCancelled:
                                    default:

                                        break;
                                        
                                    case SLComposeViewControllerResultDone:
                                        
                                        break;
                                }
                            });
         }];

        [self presentViewController:composeViewController animated:YES completion:nil];
    }
}

- (IBAction) pushedUserButton:(id) sender{
  
    if(self.userButtonEnabled)
    {
        UnitySendMessage([self.callbackGameObjectName UTF8String], "OnTapUserButton", "");

        [self close];
    }
}

- (IBAction) pushedAppButton:(id) sender{
    
    UnitySendMessage([self.callbackGameObjectName UTF8String], "OnTapAppButton", "");
        
    [self close];
}

- (IBAction) pushedLargePlayButton:(id) sender{

    [self playVideo];
}

@end
