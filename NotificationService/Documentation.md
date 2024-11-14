# Documentation
Notification Service is a Windows 10 and 11 executable microservice for displaying notifications.
The general workflow when using NotificationService is to first launch NotificationService.exe with the desired command line arguments
then send request with HTTP post.

# Command Line Args
USAGE: NotificationService.exe [/port:12345]

If the argument /port is provided then the custom port will be used.
If Notification Service fails to bind to the specified port or if the specified port is invalid then an error will be printed to the console and the process will exit with status code 0.
If Notification Service successfully binds to the specified port then the microservice will run until sent the exit command at which moment it will exit with status code 0.

If the argument /post is not provided then a random free port will be used.
Notification Service will find a free port and restart itself with that port as an argument.
The original parent process will exit with a status code which is the same as the found port.
For example if NotificationService.exe returns exit code 12345 when launched without the /port argument this means the server is now running in a new process on port 12345.

# Functions
Each function is called by sending a JSON packet with the specified arguments to a given endpoint.
If there are no input arguments then an empty JSON object should be used {}.
For example to call the ShowNotification function I would HTTP post the following JSON to http://localhost:port/ShowNotification
{
	"title":"Example Notificaion",
	"message":"Hello World This Is A Notificaion!",
	"iconPath":"C:\Users\Me\Desktop\Icon.png"
}
NotificationService.exe would then respond with a JSON status in the following form:
{
	"status":"OK"
}
Or if the function failed the response JSON might look like this:
{
	"status":"Unable to show notification because access was denied"
}

# Check
The check function always returns status OK as long as NotificationService.exe is running.
It is used to check that you have the correct port and that NotificationService.exe is running properly.
Inputs: None
Outputs: status

# Exit
The exit function instructs NotificationService.exe to exit after this request.
After sending this request no other requests may be sent.
Inputs: None
Outputs: status

# ShowNotification
The ShowNotificaion function sends the user a windows notification.
Inputs:
	title // The title of the notification.
	message // The body aka message of the notification.
	iconPath // A fully rooted path to a PNG or JPEG to be used as the notification's icon
Outputs: status