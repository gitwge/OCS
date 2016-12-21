using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPhysics
{
    public interface IPhysics_Command
    {
        string toJavaScript();
    }

    public class CommandBase : IPhysics_Command
    {
        string javaScriptCmd;

        public CommandBase(string input)
        {
            javaScriptCmd = input;
        }

        string IPhysics_Command.toJavaScript()
        {
            return javaScriptCmd;
        }
    }

    public class ImportDocument : CommandBase
    {
        public ImportDocument(string documentPath)
            : base("loadDocument('" + documentPath.Replace("\\", "/") + "');view3D.zoomAll();")
        {
        }
    }

    public class LoadDocument : CommandBase
    {
        public LoadDocument(string documentPath, bool overwrite = false) 
            : base("application.newDocument(" + overwrite.ToString().ToLower() + ");loadDocument('" + documentPath.Replace("\\", "/") + "');view3D.zoomAll();")
        {
        }
    }

    // Remote functions starting with B
    public class BeginCommandGroup : CommandBase
    {

        public BeginCommandGroup()
            : base("beginCommandGroup();")
        {
        }

        public BeginCommandGroup(String text)
            : base("beginCommandGroup('" + text + "')")
        {
        }
    }

    // Remote functions starting with C

    public class ChangeCOMTrafo : CommandBase
    {
        public ChangeCOMTrafo(String path, Vec3 position)
               :base("changeCOMTrafo('" + path + "', " + position.ToIPhysicsString() + ")")
        {
        }
    }

    public class ChangeLocalTrafo : CommandBase
    {
        public ChangeLocalTrafo(String path, Vec3 position)
               :base("changeLocalTrafo('" + path + "', " + position.ToIPhysicsString() + ")")
        {
        }
    }

    public class ChangeWorldTrafo : CommandBase
    {
        public ChangeWorldTrafo(String path, Vec3 position)
               :base("changeWorldTrafo('" + path + "', " + position.ToIPhysicsString() + ")")
        {
        }
    }

    public class CreateBox : CommandBase
    {
        public CreateBox(String path, Vec3 position)
            : base("createBox('" + path + "', " + position.ToIPhysicsString() + ")")
        {
        }

        public CreateBox(String path, Vec3 position, Vec3 dimension)
            : base("createBox('" + path + "', " + position.ToIPhysicsString() + ", " + dimension.ToIPhysicsString() + ")")
        {
        }
    }

    public class CreateCylinder : CommandBase
    {
        // public void createCylinder(String path, Trafo position, int radius, int halfHeight, MainDirection axisDirection)

        public CreateCylinder(String path, Vec3 position, int radius, int halfHeight)
            : base("createCylinder('" + path + "', " + position.ToIPhysicsString() + ", " + radius.ToString() + ", " + halfHeight.ToString() + ")")
        {
        }

        public CreateCylinder(String path, Vec3 position, int radius)
             : base("createCylinder('" + path + "', " + position.ToIPhysicsString() + ", " + radius.ToString() + ")")
        {
        }

        public CreateCylinder(String path, Vec3 position)
            :base("createCylinder('" + path + "', " + position.ToIPhysicsString() + ")")
        {
        }

        public CreateCylinder(String path)
            :base("createCylinder('" + path + "')")
        {
        } 
    }

    public class CreateCone : CommandBase
    {
        public CreateCone(String path, Vec3 position, int radius, int height, MainDirection AxisDirection) : 
            base("createCone('" + path + "', " + position.ToIPhysicsString() + ", " + radius.ToString() + ", " + height.ToString() + ", " + ((int)AxisDirection).ToString() + ")")
        {
        }

        public CreateCone(String path, Vec3 position, int radius, int height) : 
            base ("createCone('" + path + "', " + position.ToIPhysicsString() + ", " + radius.ToString() + ", " + height.ToString() + ")")
        {
        }

        public CreateCone(String path, Vec3 position, int radius) : 
            base("createCone('" + path + "', " + position.ToIPhysicsString() + ", " + radius.ToString() + ")")
        {
        }

        public CreateCone(String path, Vec3 position) : 
            base("createCone('" + path + "', " + position.ToIPhysicsString() + ")")
        {
        }

        public CreateCone(String path) : 
            base ("createCone('" + path + "')")
        {
        }
    }

    public class CreateSphere : CommandBase
    {
        public CreateSphere(String path, Vec3 position, int radius) : 
            base("createSphere('" + path + "', " + position.ToIPhysicsString() + ", " + radius.ToString() + ")")
        {
        }

        public CreateSphere(String path, Vec3 position) :
            base("createSphere('" + path + "'," + position.ToIPhysicsString() + ")")
        {
        }

        public CreateSphere(String path) :
            base("createSphere('" + path + "')")
        {
        }
    }

    public class CreateTrafo : CommandBase
    {
        public CreateTrafo(String path, Vec3 position) :
            base("createTrafo('" + path + "', " + position.ToIPhysicsString() + ")")
        {
        }

        public CreateTrafo(String path) :
            base("createTrafo('" + path + "')")
        {
        }
    }

    // Remote functions starting with R
    public class RemoveCustomProperty : CommandBase
    {
        public RemoveCustomProperty(String path, String name, String paths, String[] nameArray) : 
            base("removeCustomProperty('" + path + ", " + name + ", " + paths + ", " + nameArray.ToString() + ")")
        {
        }

        public RemoveCustomProperty(String paths, String name) :
            base("removeCustomProperty('" + paths + "', '" + name + "')")
        {
        }
    }

    public class RemoveOriginalMesh : CommandBase
    {
        public RemoveOriginalMesh(String paths) : 
            base("removeOriginalMesh('" + paths + "')")
        {
        }
    }

    public class SetBoxDimension : CommandBase
    {
        public SetBoxDimension(String path, int x, int y, int z) :
            base("setBoxDimension('" + path + "', " + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ")")
        {
        }

        public SetBoxDimension(String path, Vec3 dimension) :
            base("setBoxDimension('" + path + "', " + dimension.ToIPhysicsString() + ")")
        {
        }
    }

    public class SetCollisionMargin : CommandBase
    {
        public SetCollisionMargin(String paths, double margin) :
            base("setCollisionMargin('" + paths + "', " + margin.ToString() + ")")
        {
        }
    }

    public class SetCollisionType : CommandBase
    {
        public SetCollisionType(String path, CollisionType type) : 
            base("setCollisionType('" + path + "', " + ((int)type).ToString() + ")")
        {
        }
    }

    public class SetColor : CommandBase
    {
        public SetColor(String path, Color color) :
            base("setColor('" + path + "', " + color.ToString() + ")")
        {
        }

        public SetColor(String path, RenderMaterial material) : 
            base("setColor('" + path + "'," + material.ToIPhysicsString() + ")")
        {
        }
    }

    public class SetKinematicAxisValue : CommandBase
    {
        public SetKinematicAxisValue(String path, int value) :
            base("setKinematicAxisValue('" + path + "', " + value.ToString() + ")")
        {
        }
    }

    public class SetKinematicGearBoxInfo : CommandBase
    {
        public SetKinematicGearBoxInfo(String paths, String camInfo, String inputSwitches) : 
            base("setKinematicGearBoxInfo('" + paths + "', '" + camInfo + "', '" + inputSwitches + "')")
        {
        }

        public SetKinematicGearBoxInfo(String paths, String camInfo) :
            base("setKinematicGearBoxInfo('" + paths + "', '" + camInfo + "')")
        {
        }
    }

    public class SetKinematicSwitchInfo : CommandBase
    {
        public SetKinematicSwitchInfo(String paths, String switchInfo) : 
            base("setKinematicSwitchInfo('" + paths + "', '" + switchInfo + "')")
        {
        }
    }

    public class SetCustomProperty : CommandBase
    {
        public SetCustomProperty(String paths, String name, String value) :
            base("setCustomProperty('" + paths + "', '" + name + "', " + value + ")")
        {
        }
    }

    public class SetScriptText : CommandBase
    {
        public SetScriptText(String paths, String scriptText) :
            base("setScriptText('" + paths + "', '" + scriptText + "')")
        {
        }
    }

    public class SetTrimeshCompressionRatio : CommandBase
    {

        public SetTrimeshCompressionRatio(String paths, int reduction, int maxDeviation, bool showOrigin) : 
            base("setTrimeshCompressionRatio('" + paths + "', " + reduction.ToString() + ", " + maxDeviation.ToString() + ", " + showOrigin.ToString() + ")")
        {
        }

        public SetTrimeshCompressionRatio(String paths, int reduction, int maxDeviation) : 
            base("setTrimeshCompressionRatio('" + paths + "', " + reduction.ToString() + ", " + maxDeviation.ToString() + ")")
        {
        }
    }

    public class SetupMotionControl : CommandBase
    {
        public SetupMotionControl(String path, bool deleteIt) : 
            base("setupMotionControl('" + path + "', " + deleteIt.ToString() + ")")
        {
        }
    }

    public class StoreConfigFiles : CommandBase
    {
        public StoreConfigFiles(String name, String data) :
            base("storeConfigFiles('" + name + "', " + data + ")")
        {
        }

        public StoreConfigFiles(String name) :
            base("storeConfigFiles('" + name + "')")
        {
        }
    }

    public class SetIOValue : CommandBase
    {
       public SetIOValue(string componentPath, string variableName, string value)
            : base("setIOValue('" + componentPath + "', '" + variableName + "', " + value + ");")
        {
        }
    }

}
