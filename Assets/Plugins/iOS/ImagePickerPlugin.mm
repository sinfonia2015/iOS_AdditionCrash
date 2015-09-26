#import "ImagePickerPlugin.h"
#import "FASImagePickerController.h"
#import <MediaPlayer/MediaPlayer.h>
#import <MobileCoreServices/MobileCoreServices.h>

extern UIViewController *UnityGetGLViewController();

extern UIView *UnityGetGLView();

extern "C" void UnitySendMessage(const char* obj, const char* method, const char* msg);

typedef struct {
    unsigned char r, g, b, a;
} Color32;

static const char *strSourceType_PhotoLibrary = "PhotoLibrary";
static const char *strSourceType_MovieLibrary = "MovieLibrary";
static const char *strSourceType_Camera = "Camera";
static const char *strSourceType_SavedPhotosAlbum = "SavedPhotosAlbum";

static const char *strCallbackResultMessage_Loaded = "Loaded";
static const char *strCallbackResultMessage_Canceled = "Canceled";
static const char *strCallbackResultMessage_Hidden = "Hidden";
static const char *strCallbackResultMessage_SourceTypeUnavailable = "SourceTypeUnavailable";

static UIImagePickerControllerSourceType sourceType = UIImagePickerControllerSourceTypePhotoLibrary;

static struct {
    char strGameObjectName[256];
    char strMethodName[256];
} callbackLoadedInfo = {0};

static void ImagePicker_callback(const char *msg);

static int pickerType = 0; // 0 = photo, 1 = movie, 2 = camera

@interface ImagePickerPlugin (Private)

+ (ImagePickerPlugin *) instance;

- (bool) showImagePicker:(const char *)sourceTypeText;
- (bool) getLoadedTexrure:(Color32 *)pixelBuffer width:(int)width height:(int)height;

- (void) releaseImage;
- (void) releasePicker;
- (void) hide;

@end

@implementation ImagePickerPlugin

static MPMoviePlayerViewController *moviePlayer = nil;
static FASImagePickerController* imagePicker = nil;
static UIImage *loadedImage = nil;
static char latestVideoUrl[1024];

static ImagePickerPlugin *pInstance = nil;

+ (ImagePickerPlugin *) instance {
    if (pInstance == nil) {
        pInstance = [[ImagePickerPlugin alloc] init];
    }
    return pInstance;
}

- (ImagePickerPlugin *) init
{
    [super init];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(handleThumbnailFinished:)
                                                 name:MPMoviePlayerThumbnailImageRequestDidFinishNotification
                                               object:nil];
    
    return self;
}

- (void) releaseImage {
    if (loadedImage) {
        [loadedImage release];
        loadedImage = nil;
    }
}
- (void) releasePicker {
    if (imagePicker) {
        imagePicker.delegate = nil;
        [imagePicker release];
        imagePicker = nil;
    }
    
}

- (void)dealloc {
    
    [self releaseImage];
    [self releasePicker];
    
    [super dealloc];
    
    pInstance = nil;
}

- (bool) getLoadedTexrure:(Color32 *)pixelBuffer width:(int)width height:(int)height {
    
    assert(pixelBuffer);
    if (pixelBuffer) {
        if (loadedImage) {
            
            // Resize
            UIGraphicsBeginImageContext(CGSizeMake(width, height));
            CGRect drawRect = CGRectMake(0, 0, width, height);
            [loadedImage drawInRect:drawRect];
            UIImage *newImage = UIGraphicsGetImageFromCurrentImageContext();
            int newImageHeight = (int)newImage.size.height;
            UIGraphicsEndImageContext();
            
            // Pixel Data
            CGDataProviderRef dataProvider = CGImageGetDataProvider(newImage.CGImage);
            NSData *data = (NSData *) CFBridgingRelease(CGDataProviderCopyData(dataProvider));
            if (data) {
                assert([data length] >= (width*height*4));
                int newImagePitch = [data length] / newImageHeight / 4;
                const Color32 *pSrcBase = (const Color32 *)[data bytes];
                for (int y=0; y<height; ++y) {
                    const Color32 *pSrc = &pSrcBase[newImagePitch*y];
                    Color32 *pDst = &pixelBuffer[width*((height-1)-y)];
                    for (int x=0; x<width; ++x) {
                        pDst->r = pSrc->b;
                        pDst->g = pSrc->g;
                        pDst->b = pSrc->r;
                        pDst->a = pSrc->a;
                        ++pSrc;
                        ++pDst;
                    }
                }
            }
            
            return true;
        }
    }
    return false;
}

- (bool) showImagePicker:(const char *)sourceTypeText orientation:(int)orientation
{
    [self hide];
    
    [self releaseImage];
    
    [self releasePicker];
    
    sourceType = UIImagePickerControllerSourceTypePhotoLibrary;
    NSArray *mediaTypes = [NSArray arrayWithObject:(NSString *)kUTTypeImage];
    
    if (strcmp(sourceTypeText, strSourceType_PhotoLibrary) == 0)
    {
        pickerType = 0;
        
        sourceType = UIImagePickerControllerSourceTypePhotoLibrary;
    }
    else if (strcmp(sourceTypeText, strSourceType_MovieLibrary) == 0)
    {
        pickerType = 1;
        sourceType = UIImagePickerControllerSourceTypeSavedPhotosAlbum;
        mediaTypes = [NSArray arrayWithObject:(NSString *)kUTTypeMovie];
    }
    else if (strcmp(sourceTypeText, strSourceType_Camera) == 0)
    {
        pickerType = 2;
        
        sourceType = UIImagePickerControllerSourceTypeCamera;
    }
    else if (strcmp(sourceTypeText, strSourceType_SavedPhotosAlbum) == 0)
    {
        sourceType = UIImagePickerControllerSourceTypeSavedPhotosAlbum;
    }
    
    if ([UIImagePickerController isSourceTypeAvailable:sourceType] == false)
    {
        ImagePicker_callback(strCallbackResultMessage_SourceTypeUnavailable);
        
        return false;
    }
    
    imagePicker = [[FASImagePickerController alloc] init];
    imagePicker.sourceType = sourceType;
    imagePicker.mediaTypes = mediaTypes;
    imagePicker.videoQuality = UIImagePickerControllerQualityType640x480;
    imagePicker.allowsEditing = NO;
    imagePicker.delegate = self;
    
    [UnityGetGLViewController() presentViewController:imagePicker animated:YES completion:^ {}];
    
    return true;
}

- (void) hide
{
    [UnityGetGLViewController() dismissViewControllerAnimated:YES completion:^ {}];
}

- (void) handleThumbnailFinished:(NSNotification*)notification
{
    NSDictionary *userInfo = [notification userInfo];
    loadedImage = [userInfo valueForKey:MPMoviePlayerThumbnailImageKey];
    if (loadedImage)
        [loadedImage retain];
    moviePlayer = nil;
    
    ImagePicker_callback(strCallbackResultMessage_Loaded);
    
    [self hide];
    
    ImagePicker_callback(strCallbackResultMessage_Hidden);
}

- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info
{
    [self releaseImage];
    
    if(pickerType == 1) { // movie
        
        NSURL *url = [info objectForKey:UIImagePickerControllerReferenceURL];
        
        if (url)
        {
            NSURL *videoUrl = [info objectForKey:UIImagePickerControllerMediaURL];
            
            NSString *videoPath = [videoUrl path];
            
            strncpy(latestVideoUrl, videoPath.UTF8String, sizeof(latestVideoUrl));
            
            moviePlayer = [[MPMoviePlayerViewController alloc] initWithContentURL:url];
            [[moviePlayer moviePlayer] requestThumbnailImagesAtTimes:[NSArray arrayWithObject:[NSNumber numberWithFloat:0.0f]] timeOption:MPMovieTimeOptionNearestKeyFrame];
        }
    }
    else
    {
        loadedImage = [info objectForKey:UIImagePickerControllerEditedImage];
        
        if (loadedImage == nil) {
            loadedImage = [info objectForKey:UIImagePickerControllerOriginalImage];
        }
        
        if (loadedImage) {
            [loadedImage retain];
        }
        
        UIImage *resizedImage;
        
        if(loadedImage.size.width > loadedImage.size.height && loadedImage.size.width > 1024){
            
            float scale = 1024.0f / loadedImage.size.width;
            
            CGSize resizedSize = CGSizeMake(loadedImage.size.width * scale, loadedImage.size.height * scale);
            
            UIGraphicsBeginImageContext(resizedSize);
            
            [loadedImage drawInRect:CGRectMake(0, 0, resizedSize.width, resizedSize.height)];
            
            resizedImage = UIGraphicsGetImageFromCurrentImageContext();
            
            UIGraphicsEndImageContext();
        }
        else if(loadedImage.size.width < loadedImage.size.height && loadedImage.size.height > 1024){
            
            float scale = 1024.0f / loadedImage.size.height;
            
            CGSize resizedSize = CGSizeMake(loadedImage.size.width * scale, loadedImage.size.height * scale);
            
            UIGraphicsBeginImageContext(resizedSize);
            
            [loadedImage drawInRect:CGRectMake(0, 0, resizedSize.width, resizedSize.height)];
            
            resizedImage = UIGraphicsGetImageFromCurrentImageContext();
            
            UIGraphicsEndImageContext();
        }
        else{
            resizedImage = loadedImage;
        }
        
        UIImage *rotatedImage;
        
        if (resizedImage.imageOrientation == UIImageOrientationDown) {
            rotatedImage = [UIImage imageWithCGImage:[[self class] CGImageRotatedByAngle:[[self class] CGImageRotatedByAngle:resizedImage.CGImage angle:90] angle:90]];
        }
        else if(resizedImage.imageOrientation == UIImageOrientationLeft){
            rotatedImage = [UIImage imageWithCGImage:[[self class] CGImageRotatedByAngle:resizedImage.CGImage angle:90]];
        }else if(resizedImage.imageOrientation == UIImageOrientationRight){
            rotatedImage = [UIImage imageWithCGImage:[[self class] CGImageRotatedByAngle:resizedImage.CGImage angle:-90]];
        }else{
            rotatedImage = [UIImage imageWithCGImage:resizedImage.CGImage];
        }
        
        NSData *imageData = UIImageJPEGRepresentation(rotatedImage, 1.0);
        
        NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
        
        NSString *documentsDirectory = [paths objectAtIndex:0];
        
        NSString *filePath = [documentsDirectory stringByAppendingPathComponent:@"PickedImage.jpg"];
        
        //file name
        NSLog(@"filePath %@",filePath);
        
        [imageData writeToFile:filePath atomically:YES];
        
        //ImagePicker_callback(strCallbackResultMessage_Loaded);
        
        ImagePicker_callback([filePath UTF8String]);
        
        [self hide];
        
        ImagePicker_callback(strCallbackResultMessage_Hidden);
    }
}

+ (CGImageRef)CGImageRotatedByAngle:(CGImageRef)imgRef angle:(CGFloat)angle
{
    CGFloat angleInRadians = angle * (M_PI / 180);
    CGFloat width = CGImageGetWidth(imgRef);
    CGFloat height = CGImageGetHeight(imgRef);
    
    CGRect imgRect = CGRectMake(0, 0, width, height);
    CGAffineTransform transform = CGAffineTransformMakeRotation(angleInRadians);
    CGRect rotatedRect = CGRectApplyAffineTransform(imgRect, transform);
    CGColorSpaceRef colorSpace = CGColorSpaceCreateDeviceRGB();
    CGContextRef bmContext = CGBitmapContextCreate(NULL,
                                                   rotatedRect.size.width,
                                                   rotatedRect.size.height,
                                                   8,
                                                   0,
                                                   colorSpace,
                                                   (CGBitmapInfo)kCGImageAlphaPremultipliedFirst);
    CGContextSetInterpolationQuality(bmContext, kCGInterpolationNone);
    CGColorSpaceRelease(colorSpace);
    CGContextTranslateCTM(bmContext,
                          +(rotatedRect.size.width/2),
                          +(rotatedRect.size.height/2));
    CGContextRotateCTM(bmContext, angleInRadians);
    CGContextTranslateCTM(bmContext,
                          -(rotatedRect.size.height/2),
                          -(rotatedRect.size.width/2));
    CGContextDrawImage(bmContext, CGRectMake(0, 0,
                                             rotatedRect.size.height,
                                             rotatedRect.size.width),
                       imgRef);
    
    CGImageRef rotatedImage = CGBitmapContextCreateImage(bmContext);
    CFRelease(bmContext);
    bmContext = nil;
    
    return rotatedImage;
}

- (void) imagePickerControllerDidCancel:(UIImagePickerController*)picker
{
    [self hide];
    
    ImagePicker_callback(strCallbackResultMessage_Canceled);
}

@end

// Interface
extern "C" bool ImagePicker_showPicker(const char *sourceType, int orientation, const char *callbackGameObjectName, const char *callbackMethodName) {
    
    //showOrientation = orientation;
    
    strncpy(callbackLoadedInfo.strGameObjectName, callbackGameObjectName, sizeof(callbackLoadedInfo.strGameObjectName));
    
    strncpy(callbackLoadedInfo.strMethodName, callbackMethodName, sizeof(callbackLoadedInfo.strMethodName));
    
    return [[ImagePickerPlugin instance] showImagePicker:sourceType orientation:orientation];
}

extern "C" bool ImagePicker_isLoaded() {
    return (loadedImage);
}

extern "C" int ImagePicker_getLoadedTexrureWidth() {
    if (ImagePicker_isLoaded()) {
        return (int)[loadedImage size].width;
    }
    return 0;
}

extern "C" int ImagePicker_getLoadedTexrureHeight() {
    if (ImagePicker_isLoaded()) {
        return (int)[loadedImage size].height;
    }
    return 0;
}

extern "C" bool ImagePicker_getLoadedTexrure(Color32 *pixelBuffer, int width, int height) {
    return [[ImagePickerPlugin instance] getLoadedTexrure:pixelBuffer width:width height:height];
}

extern "C" void ImagePicker_release() {
    [[ImagePickerPlugin instance] release];
}

extern "C" void ImagePicker_releaseLoadedImage() {
    [[ImagePickerPlugin instance] releaseImage];
}

extern "C" void ImagePicker_savePngDataToPhotoLibrary(const char** ptrImageData, const int imageDataLength){
    
    NSData *imgData = [NSData dataWithBytes:(const void *)ptrImageData length:(sizeof(unsigned char) * imageDataLength)];
    UIImage *image = [UIImage imageWithData:imgData];
    UIImageWriteToSavedPhotosAlbum(image, nil, nil, nil);
    
}

extern "C" char *ImagePicker_getVideoUrl(){
    
    assert(latestVideoUrl);
    char *copyStr = (char *)malloc(strlen(latestVideoUrl)+1);
    assert(copyStr);
    strcpy(copyStr, latestVideoUrl);
    return copyStr;
}

static char *fasStrCopy(const char *str) {
    assert(str);
    char *copyStr = (char *)malloc(strlen(str)+1);
    assert(copyStr);
    strcpy(copyStr, str);
    return copyStr;
}

static void ImagePicker_callback(const char *msg) {
    UnitySendMessage(fasStrCopy(callbackLoadedInfo.strGameObjectName), fasStrCopy(callbackLoadedInfo.strMethodName), fasStrCopy(msg));
}

