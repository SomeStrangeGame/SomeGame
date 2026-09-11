using System;
using UnityEditor;
using UnityEngine;

public static class ApplicationUpdateValidation
{
    [MenuItem("Tools/Novels/Validate Direct APK Update Contract")]
    public static void Validate()
    {
        Require(Novels.ApplicationUpdatePolicy.FileName("kostroma-dev")
            == "updates/kostroma-dev.json", "Update manifest path differs.");
        Require(Novels.DirectApkUpdater.TryCreate(
            "https://pureshechka.com/DevBuilds/Kostroma-3.apk", 1024,
            new string('a', 64), 3) != null, "Valid direct update was rejected.");
        Require(Novels.DirectApkUpdater.TryCreate(
            "http://pureshechka.com/app.apk", 1024, new string('a', 64), 3) == null,
            "Non-HTTPS update was accepted.");
        Require(Novels.DirectApkUpdater.TryCreate(
            "https://pureshechka.com/app.apk", 0, new string('a', 64), 3) == null,
            "Zero-sized update was accepted.");
        Require(Novels.DirectApkUpdater.TryCreate(
            "https://pureshechka.com/app.apk", 1024, "invalid", 3) == null,
            "Invalid SHA-256 was accepted.");

        var action = Novels.DirectApkUpdater.TryCreate(
            "https://pureshechka.com/app.apk", 1024, new string('0', 64), 3);
        var prompt = new Novels.Catalog.CatalogUpdatePrompt(
            Novels.Catalog.CatalogUpdateMode.Hard, string.Empty, action);
        Require(prompt.IsVisible, "Action-backed update prompt is hidden.");
        Debug.Log("Direct APK update contract validation passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
