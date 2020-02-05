using Godot;
using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Field)]
public class BindNodeBase : Attribute {
    public string Path { set; get; }
    public bool Root { set; get; }

    public BindNodeBase(string path = "", bool root = false) {
        this.Path = path;
        this.Root = root;
    }

    public string GetNodePath(FieldInfo field) {
        var bindPath = "";

        // Insert /root/
        if (Root) {
            bindPath += "/root/";
        }

        // No path bind, return type name
        if (Path == "") {
            bindPath += field.FieldType.Name;
        } else {
            bindPath += Path;
        }

        return bindPath;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class BindNode : BindNodeBase {
    public BindNode(string path = "") : base(path, false) { }
}

[AttributeUsage(AttributeTargets.Field)]
public class BindNodeRoot : BindNodeBase {
    public BindNodeRoot(string path = "") : base(path, true) { }
}

public static class NodeExt {
    /// <summary>
    /// Automatically bind nodes with a BindNodeBase attribute.
    /// BindNodeBase implementations are <c>BindNode</c> and <c>BindNodeRoot</c>.
    /// <c>BindNode</c> will get a node corresponding to the type name, unless a path is explicitly given.
    /// <c>BindNodeRoot</c> will get a node from the root element corresponding to the type name,
    /// unless a path is explicitly given.
    /// </summary>
    public static void BindNodes<T>(this T node) where T : Node {
        var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields) {
            var customAttr = (BindNodeBase)field.GetCustomAttribute(typeof(BindNodeBase));
            if (customAttr != null) {
                var bindPath = customAttr.GetNodePath(field);

                // Bind
                var nodeInstance = node.GetNode(bindPath);
                field.SetValue(node, nodeInstance);
            }
        }
    }

    /// <summary>
    /// Instantiate a PackedScene with a known Node type.
    /// </summary>
    public static T InstanceAs<T>(this PackedScene packed, PackedScene.GenEditState editState = PackedScene.GenEditState.Disabled) where T : Node {
        return (T)packed.Instance(editState);
    }

    /// <summary>
    /// Get a DynamicFont from a Control node.
    /// </summary>
    public static DynamicFont GetDynamicFont(this Control control, string name, string type = "") {
        return (DynamicFont)control.GetFont(name, type);
    }
}
