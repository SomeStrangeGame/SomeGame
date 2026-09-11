package com.pureshechka.novels.update;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.provider.Settings;

import java.io.File;
import java.security.MessageDigest;
import java.util.HashSet;
import java.util.Set;

public final class DirectApkUpdater {
    private DirectApkUpdater() {}

    public static int getInstalledVersionCode(Activity activity) {
        try {
            PackageInfo info = activity.getPackageManager()
                    .getPackageInfo(activity.getPackageName(), 0);
            return Build.VERSION.SDK_INT >= 28 ? (int) info.getLongVersionCode() : info.versionCode;
        } catch (Exception ignored) { return 0; }
    }

    public static boolean canRequestPackageInstalls(Activity activity) {
        return Build.VERSION.SDK_INT < 26
                || activity.getPackageManager().canRequestPackageInstalls();
    }

    public static void openUnknownSourcesSettings(Activity activity) {
        Intent intent = new Intent(Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES,
                Uri.parse("package:" + activity.getPackageName()));
        activity.startActivity(intent);
    }

    public static String verifyApk(Activity activity, String path, int expectedVersionCode) {
        try {
            PackageManager manager = activity.getPackageManager();
            int flags = Build.VERSION.SDK_INT >= 28
                    ? PackageManager.GET_SIGNING_CERTIFICATES : PackageManager.GET_SIGNATURES;
            PackageInfo candidate = manager.getPackageArchiveInfo(path, flags);
            PackageInfo installed = manager.getPackageInfo(activity.getPackageName(), flags);
            if (candidate == null) return "unreadable_package";
            if (!activity.getPackageName().equals(candidate.packageName)) return "package_name";
            long candidateVersion = Build.VERSION.SDK_INT >= 28
                    ? candidate.getLongVersionCode() : candidate.versionCode;
            long installedVersion = Build.VERSION.SDK_INT >= 28
                    ? installed.getLongVersionCode() : installed.versionCode;
            if (candidateVersion != expectedVersionCode || candidateVersion <= installedVersion)
                return "version_code";
            Set<String> expected = signerDigests(installed);
            Set<String> actual = signerDigests(candidate);
            if (expected.isEmpty() || actual.isEmpty() || !expected.equals(actual))
                return "signing_certificate";
            return "";
        } catch (Exception exception) {
            return "inspection_failed";
        }
    }

    private static Set<String> signerDigests(PackageInfo info) throws Exception {
        android.content.pm.Signature[] signatures;
        if (Build.VERSION.SDK_INT >= 28) {
            signatures = info.signingInfo.hasMultipleSigners()
                    ? info.signingInfo.getApkContentsSigners()
                    : info.signingInfo.getSigningCertificateHistory();
        } else {
            signatures = info.signatures;
        }
        Set<String> values = new HashSet<>();
        MessageDigest digest = MessageDigest.getInstance("SHA-256");
        if (signatures != null)
            for (android.content.pm.Signature signature : signatures)
                values.add(hex(digest.digest(signature.toByteArray())));
        return values;
    }

    private static String hex(byte[] bytes) {
        StringBuilder value = new StringBuilder(bytes.length * 2);
        for (byte item : bytes) value.append(String.format("%02x", item & 0xff));
        return value.toString();
    }

    public static void installApk(Activity activity, String path) {
        Uri uri = new Uri.Builder()
                .scheme("content")
                .authority(activity.getPackageName() + ".directapkupdate")
                .appendPath("update.apk")
                .build();
        Intent intent = new Intent(Intent.ACTION_VIEW)
                .setDataAndType(uri, "application/vnd.android.package-archive")
                .addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
                .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        activity.startActivity(intent);
    }
}
