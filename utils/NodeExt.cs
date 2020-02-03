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
}
