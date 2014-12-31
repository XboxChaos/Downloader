Xbox Chaos Downloader
=====================

A tool for automatically downloading and setting up pre-compiled applications registered with the [Xbox Chaos API](https://github.com/XboxChaos/XboxChaosApi).

_Please note that this is still a work in progress and that not all features have been fully implemented yet._

Configuring
-----------

To use the downloader, you need to create an `app.json` file in the same folder as the executable. This file contains information about the application to be downloaded and how the downloader should operate. Here is a sample file for [Assembly](https://github.com/xboxchaos/assembly):
```json
{
	"application_name": "Assembly",
	"default_branch": "master",
	"quick": false
}
```
The keys work as follows:

Key | Description
--- | -----------
application_name | The internal name of the application to request from the API.
default_branch | The name of the branch to select by default.
quick | _Not implemented yet._ Whether or not "quick mode" should be activated. If this is set to true, then the user will never be prompted for input. The default branch for the application will be downloaded and installed in the same folder as the downloader.
