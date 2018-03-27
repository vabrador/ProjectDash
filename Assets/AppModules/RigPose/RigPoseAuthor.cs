using Leap.Unity;
using Leap.Unity.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Dash {

  [ExecuteInEditMode]
  public class RigPoseAuthor : MonoBehaviour {

    [Header("Working Asset")]

    public AssetFolder workingDir;

    [QuickButton("Save", "SaveWorkingAsset")]
    [OnEditorChange("workingName")]
    [SerializeField]
    private string _workingName;
    public string workingName {
      get { return _workingName; }
      set {
        _workingName = value;
        refreshExistingAsset();
      }
    }

    public string workingAssetPath {
      get {
        return Path.Combine(workingDir.Path, Path.ChangeExtension(workingName, ".asset"));
      }
    }

    [QuickButton("Load", "LoadWorkingAsset")]
    [SerializeField]
    private RigPoseAsset _existingAsset;
    public bool hasExistingAsset { get { return _existingAsset != null; } }

    //[QuickButton("Save", "SaveWorkingAsset")]
    public RigPose currRigPose = new RigPose();

    [Header("Bone/Transform Linkage")]

    [Tooltip("White-delimited, case-insensitive tokens that must match Transform names "
           + "in order to be saved by the rig. If this value is null, all bones are "
           + "considered for the rig (use boneExcludeTokens for exceptions).")]
    public string boneIncludeTokens = "";
    private List<string> _includeTokens = new List<string>();

    [Tooltip("White-delimited, case-insensitive tokens that prevent Transforms with "
           + "matching names from being linked to this rig.")]
    public string boneExcludeTokens = "Cube Sphere";
    private List<string> _excludeTokens = new List<string>();

    private void Update() {
      if (!hasExistingAsset) {
        refreshExistingAsset();
      }
      else {
        updateCurrentRigWithTransforms();
      }
    }

    public void SaveWorkingAsset() {
      tryEnsureWorkingAssetExists();

      updateCurrentRigWithTransforms();

      if (hasExistingAsset) {
        _existingAsset.rigPose.Fill(currRigPose);
      }

      AssetDatabase.SaveAssets();
    }

    public void LoadWorkingAsset() {
      if (!hasExistingAsset) {
        Debug.LogWarning("[RigPoseAuthor] Cannot load. Working asset " + workingName
          + " does not exist in directory " + workingDir);
        return;
      }

      currRigPose.Fill(_existingAsset.rigPose);

      updateTransformsWithCurrentRig();
    }

    private void refreshExistingAsset() {
      if (!string.IsNullOrEmpty(workingName)) {
        _existingAsset
          = AssetDatabase.LoadAssetAtPath(workingAssetPath, typeof(RigPoseAsset))
            as RigPoseAsset;
      }
      else {
        _existingAsset = null;
      }
    }

    private void tryEnsureWorkingAssetExists() {
      if (hasExistingAsset) return;
      else if (string.IsNullOrEmpty(workingName)) return;
      else {
        var newRigPoseAsset = ScriptableObject.CreateInstance<RigPoseAsset>();
        newRigPoseAsset.rigPose = currRigPose;
        AssetDatabase.CreateAsset(newRigPoseAsset, workingAssetPath);
        AssetDatabase.SaveAssets();

        _existingAsset = newRigPoseAsset;
      }
    }

    private void updateTokens() {
      _includeTokens.Clear();
      foreach (var token in boneIncludeTokens.Split(
                              new char[] { ' ' },
                              System.StringSplitOptions.RemoveEmptyEntries)) {
        _includeTokens.Add(token);
      }

      _excludeTokens.Clear();
      foreach (var token in boneExcludeTokens.Split(
                              new char[] { ' ' },
                              System.StringSplitOptions.RemoveEmptyEntries)) {
        _excludeTokens.Add(token);
      }
    }

    private void updateCurrentRigWithTransforms() {
      currRigPose.Clear();

      updateTokens();

      var allChildren = Pool<List<Transform>>.Spawn(); allChildren.Clear();
      try {
        this.transform.GetAllChildren(allChildren);
        foreach (var child in allChildren) {
          bool shouldInclude = true;
          if (_includeTokens.Count > 0) {
            shouldInclude = false;
            foreach (var token in _includeTokens) {
              if (child.name.MatchesToken(token)) {
                shouldInclude = true;
                break;
              }
            }
          }

          bool shouldExclude = false;
          foreach (var token in _excludeTokens) {
            if (child.name.MatchesToken(token)) {
              shouldExclude = true;
              break;
            }
          }

          if (shouldInclude && !shouldExclude) {
            currRigPose.AddOrSetBone(
              child.GetPathFromRoot(this.transform),
              child.ToLocalPose(),
              child.localScale);
          }
        }
      }
      finally {
        allChildren.Clear(); Pool<List<Transform>>.Recycle(allChildren);
      }
    }

    private void updateTransformsWithCurrentRig() {
      currRigPose.SetTransforms(this.transform);
    }

  }

  public static class RigPoseAuthorExtensions {

    /// <summary>
    /// Depth-first scans all the children in order of the argument Transform, appending
    /// each transform it finds to toFill.
    /// </summary>
    public static void GetAllChildren(this Transform t, List<Transform> toFill) {
      addChildrenRecursive(t, toFill);
    }
    private static void addChildrenRecursive(Transform t, List<Transform> list) {
      foreach (var child in t.GetChildren()) {
        list.Add(child);
        addChildrenRecursive(child, list);
      }
    }

    /// <summary>
    /// Returns whether the input string matches the token string, meaning, whether the
    /// string contains the token. Pass ignoreCase = false to consider upper/lowercase.
    public static bool MatchesToken(this string testStr, string tokenStr,
                                    bool ignoreCase = true) {
      if (ignoreCase) {
        testStr = testStr.ToLower();
        tokenStr = tokenStr.ToLower();
      }
      return testStr.Contains(tokenStr);
    }
    
    /// <summary>
    /// Returns a '/'-delimited parent-to-child path down from the provided rootTransform,
    /// to this Transform. rootTrasform must be a parent or grandparent of this Transform,
    /// otherwise an InvalidOperationException will be thrown.
    /// </summary>
    public static string GetPathFromRoot(this Transform thisT, Transform rootTransform) {
      var nameBuffer = Pool<List<string>>.Spawn(); nameBuffer.Clear();
      StringBuilder sb = new StringBuilder();
      try {
        var curT = thisT;
        do {
          nameBuffer.Add(curT.name);
          curT = curT.parent;
          if (curT == null) {
            throw new System.InvalidOperationException(
              "rootTransform " + rootTransform.name + " is not a parent of " + thisT.name);
          }
        }
        while (curT != rootTransform);

        for (int i = nameBuffer.Count - 1; i >= 0; i--) {
          sb.Append(nameBuffer[i] + (i != 0 ? "/" : ""));
        }
        return sb.ToString();
      }
      finally {
        nameBuffer.Clear(); Pool<List<string>>.Recycle(nameBuffer);
      }
    }

    /// <summary>
    /// Inverse of GetPathFromRoot. Given a root transform and a '/'-delimited
    /// parent-to-child path from the root transform, traverses down the root transform's
    /// hierarchy and returns the child Transform specified by the path.
    /// </summary>
    public static Transform Traverse(this Transform rootTransform, string pathFromRoot) {
      var curT = rootTransform;
      var pathRemaining = pathFromRoot;
      do {
        var indexOfSeparator = pathRemaining.IndexOf('/');
        string targetName;
        if (indexOfSeparator != -1) {
          targetName = pathRemaining.Substring(0, indexOfSeparator);
          pathRemaining = pathRemaining.Substring(indexOfSeparator + 1);
        }
        else {
          targetName = pathRemaining;
          pathRemaining = "";
        }

        bool foundChild = false;
        foreach (var child in curT.GetChildren()) {
          if (child.name.Equals(targetName)) {
            curT = child;
            foundChild = true;
            break;
          }
        }
        if (!foundChild) {
          // Path is invalid -- no transform at this path.
          return null;
        }
      } while (!string.IsNullOrEmpty(pathRemaining));

      return curT;
    }
  }


}
