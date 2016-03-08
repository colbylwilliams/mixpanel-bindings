IOS_RELEASE=v2.9.4
ANDROID_RELEASE=v4.8.0

XBUILD=/Applications/Xcode.app/Contents/Developer/usr/bin/xcodebuild
PROJECT=Mixpanel.xcodeproj
TARGET=Mixpanel

GRADLE=sh /Applications/Android\ Studio.app/Contents/gradle/gradle-2.8/bin/gradle
ANDROID_HOME=$(HOME)/Library/Developer/Xamarin/android-sdk-macosx


all : clean ios/Mixpanel.framework android/mixpanel-android-release.aar

ios/mixpanel-iphone/README.md :
	mkdir -p ios
	cd ios && git clone git@github.com:mixpanel/mixpanel-iphone.git
	cd ios/mixpanel-iphone && git checkout $(IOS_RELEASE)

ios/Mixpanel.framework : ios/mixpanel-iphone/README.md
	cd ios/mixpanel-iphone && $(XBUILD) -project $(PROJECT) -target $(TARGET) -sdk iphoneos -configuration Release clean build
	-mv ios/mixpanel-iphone/build/Release-iphoneos/Mixpanel.framework ios/
	-mv ios/mixpanel-iphone/build/Release-iphoneos/Mixpanel.framework.dSYM ios/


android/mixpanel-android/README.md :
	mkdir -p android
	cd android && git clone git@github.com:mixpanel/mixpanel-android.git
	cd android/mixpanel-android && git checkout $(ANDROID_RELEASE)

android/mixpanel-android-release.aar : android/mixpanel-android/README.md
	-cd android/mixpanel-android && ANDROID_HOME=$(ANDROID_HOME) $(GRADLE) clean build aR
	-mv android/mixpanel-android/build/outputs/aar/mixpanel-android-release.aar android/
	-mv android/mixpanel-android/build/outputs/aar/mixpanel-android-debug.aar android/

clean :
	rm -rf ios
	rm -rf android