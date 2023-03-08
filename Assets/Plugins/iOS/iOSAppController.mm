#import "UnityAppController.h"

@interface iOSAppController : UnityAppController {}
@end

@implementation iOSAppController
- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application{
	printf_console("WARNING iOS APP CONTROLLER -> applicationDidReceiveMemoryWarning()\n");
	UnitySendMessage("Core", "iOSReceivedMemoryWarning", "");
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(iOSAppController)