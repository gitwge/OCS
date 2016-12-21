using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;

namespace IPhysics
{
    public class Support
    {
        protected static String getKeyName(String label)
        {
            return "Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + label;
        }

        public static string GetRegistryKey(string labelText)
        {
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default); //  = new Microsoft.Win32.RegistryKey();
            Object value = key.GetValue(Support.getKeyName(labelText));
            if (value != null)
                return value.ToString();

            return "";
        }

        public static void SetRegistryKey(string keyName, string name)
        {
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default); //  = new Microsoft.Win32.RegistryKey();
            key.SetValue(Support.getKeyName(keyName), name);
            key.Close();
        }
    }

    public enum AxisModelType
    {
        UnknownType,      //  0 - 
        KinematicAxis,    //  1 - The axis belongs to a kinematic axis drive.
        ChainAxis,        //  2 - The axis belongs to a chain object drive.
        ConveyorBeltAxis, //  3 - The Axis belongs to a conveyor belt drive.
        ConstraintAxis    //  4 - The axis belongs to a constraint drive.
    }

    public enum AxisUsageDirection
    {
        UnspecifiedUsage, //  0 - The axis has an unspecified usage (direction).
        Translational,    //  1 - The axis belongs to a drive that moves along an axis in space.
        Rotational        //  2 - The Axis belongs to a drive that rotates around an axis in space.
    }

    public enum BodyType
    {
        Immaterial,       //  0 - Immaterielle Komponenten werden in der Physiksimulation nicht berücksichtigt.
        Static,           //  1 - Statische Komponenten sind fest und stellen Störgeometrien in der Simulation dar, die nicht bewegt werden kann.
        Kinematic,        //  2 - Kinematische Komponenten haben dasselbe Verhalten wie statische Komponenten, können allerdings während der Simulation bewegt werden.
        Dynamic,          //  3 - Dynamische Komponenten haben eine Masse und unterliegen den physikalischen Gesetzmäßigkeiten, wie Schwerkraft, Reibung und Impulserhaltung.
        DynamicComponent  //  4 - Eine dynamische Unterkomponente ist Teil der nächsten übergeordneten materiellen Komponente.
    }

    public enum CollisionType
    {
        NoCollision,          //  0 - Keine Kollision
        Normal,               //  1 - Normale Kollision, die Gesetze der Physik wirken.
        StickySurface,        //  2 - Klebrige Kollision, Dynamische Komponenten kleben an anderen dynamishen, statischen und kinematischen Komponenten.
        LightBarrier,         //  3 - Kollision zur Darstellung von Lichtschranken.
        IDScanner,            //  4 - Kollision zur Nutzung als ID-Scanner.
        Tracking6D,           //  5 - Kollision genutzt zur als 6D Sensor.
        Recognition2D1D,      //  6 - Kollision als 2D / 1D sensor (Bilderkennung).
        KinematicGripper,     //  7 - Kollision zur Darstellung eines kinematischen Greifers (Keine constraints, aber kinematischer, harter Griff).
        CompliantGripper,     //  8 - Weicher Greifer (Greifer mit einer Zwangsbedingung).
        MovingSurface,        //  9 - Kollision für Förderbänder (Oberfläche mit Geschwindigkeitsvektor).
        RotatingSurface,      //  10 - Kollision für Kurvenbänder (Drehfeld auf der Oberfläche).
        TransientSource,      //  11 - Kollision genutzt als Quelle für Transiente Komponenten.
        TransientSink,        //  12 - Kollision zur Darstellung einer Senke für Transiente Komponenten.
        LiveStatistics,       //  13 - Kollision als Objektzähler mit Statistik.
        BlackBoxProcess,      //  14 - Kollision genutz für die Darstellung eines abstrakten Prozesses.
        ForceField,           //  15 - Kollision mit Kraftfeld, genutzt, um Bereiche mit einem Kraftfeld zu implementieren.
        RotatingForceField,   //  16 - Rotierendes Kraftfeld für die Darstellung von Kreisförmigen Kraftfeldern.
        PalletStacking,       //  17 - Kollision zur Darstelung von Paletten mit Bauteilstapeln.
        ShiftRegisterProcess, //  18 - Collision used to implement a shift register process (shifts incoming parts along a shift register with three phases// shift in, process, shift out)
        DistributorProcess,   //  19 - Collision used to implement a distribution process (considers certain labels on a piece and switches it to its output targets after an appropriate handling time)
        RFIDReader,           //  20 - Collision used as RFID reader. This can be used to read named properties from a dynamic or transient component.
        RFIDWriter,           //  21 - Collision used as RFID writer. This can be used to read and write named properties from a dynamic or transient component.
        GripperSuppression,   //  22 - Collision used to create a field of compliant gripper suppression. When not deactivated, this can be used to suppress compliant grippers with less priority.
        FormingProcess,       //  23 - Forming process collision. Eates a primitive geometry in the entry side and creates a new part with a certain cross section at equal rate of volume.
        ContinuousSource,     //  24 - Continuous source collision. Casts a new part with a certain cross section at a controllable speed.
        ContinuousForwarder,  //  25 - Continuous forwarder collision. Eates a continuous or primitive geometry in the entry side and casts a new part with the same cross section at speed on the outfeed side.
        ContinuousSink,       //  26 - Continuous sink collision. Eates a continuous or primitive geometry and destroys it.
        CuttingBlade,         //  27 - Cutting blade collision. Cuts fully covered components into half.
    }

    public enum CollisionBody
    {
        NoCollisionBody,     //  0 - Kein Kollisionstyp
        NativeBody,          //  1 - Nativen Körper nutzen (Dreiecksnetz --> Dreiecksnetz, Quader --> Quader, ...)
        OuterSphere,         //  2 - Hüllkugel nutzen (Vereinfachte Hüllkugel um den Hüllquader)
        InnerSphere,         //  3 - Innere Kugel nutzen (Vereinfachte innere Kugel an den Hüllquader)
        AABBBox,             //  4 - Hüllquader nutzen (Ausgerichtet an den lokalen Koordinaten)
        CylinderalongX,      //  5 - Inneren Zylinder entlang der lokalen X-Achse an den Hüllquader nutzen.
        CylinderalongY,      //  6 - Inneren Zylinder entlang der lokalen Y-Achse an den Hüllquader nutzen.
        CylinderalongZ,      //  7 - Inneren Zylinder entlang der lokalen Z-Achse an den Hüllquader nutzen.
        ConvexTriangleMesh,  //  8 - Konvexes Dreiecksnetz nutzen, das vom originalen Dreiecksnetz abgeleitet ist.
        ConvexCompound,      //  9 - Konvexe Zerlegung des originalen Dreiecksnetzes nutzen.
        ConcaveTriangleMesh, //  10 - Das originale Dreiecksnetz direkt nutzen.
        ConealongX,          //  11 - Inneren Kegel antlang der lokalen X-Achse an den Hüllquader nutzen.
        ConealongY,          //  12 - Inneren Kegel antlang der lokalen Y-Achse an den Hüllquader nutzen.
        ConealongZ,          //  13 - Inneren Kegel antlang der lokalen Z-Achse an den Hüllquader nutzen.
    }

    public enum DriveType
    {
        DirectDrive,      //  0 - 
        BitMotor,         //  1 - 
        RampedBitMotor,   //  2 - 
        PositioningMotor, //  3 - 
        PIDControlMotor,  //  4 - 
        VibrationMotor    //  5 -   
    }

    public enum KinematicAxis
    {
        NoAxis, //  0 - No axis
        TransX, //  1 - Translation along X
        TransY, //  2 - Translation along Y
        TransZ, //  3 - Translation along Z
        RotX,   //  4 - Rotation along X
        RotY,   //  5 - Rotation along Y
        RotZ,   //  6 - Rotation along Z
        Free6D  //  7 - Free6D
    }

    public enum MainDirection
    {
        XAxis,
        YAxis,
        ZAxis
    }

    public enum MovieEncoderFlags
    {
        None = 0,                   //  0 - No particular settings chosen.
        Resize = 1,                 //  1 - If set, the resolution of the movie is resized.
        AddTime = 2,                //  2 - If set, a time display is added to the output movie.
        SuppressModalMessages = 4,  //  4 - If set, no modal messages will appear.
        SuppressModalProgress = 8,  //  8 - Currently not implemented (If set, no modal progress will appear).
        JSONProgress = 16,          //  16 - If set, a JSON progress will ne sent to the js server.
        JSONMessages = 32,          //  32 - If set, messages will be written to the console as well as sent to the js server.
    }

    public enum SpeedDriveType
    {
        DirectSpeedMotor,
        RampedBitSpeedMotor,
        RampedSpeedMotorWithCounter,
        PositioningSpeedMotor
    }

    public struct AxisInfo
    {
        String name;
        String path;
        String guid;
        String type;
        String usage;
        String level;

        public override String ToString()
        {
            return "AxisInfo(" + name + "," + path + "," + guid + "," + type + "," + usage + "," + level + ")";
        }
    }

    public struct BlackBoxProcessInfo
    {
        Trafo outputPosition;
        double avgProcessTime;
        double processTimeVariance;
        double bufferDirection;
        double bufferPosition;

        public override String ToString()
        {
            return "BlackBoxProcessInfo(" + outputPosition.ToIPhysicsString() + ", " + avgProcessTime.ToString() + ", "
                + processTimeVariance.ToString() + ", " + bufferDirection.ToString() + ", " + bufferPosition.ToString() + ")";
        }
    }

    public struct CamInfo
    {
        String name;
        String path;
        String curve;

        public override String ToString()
        {
            return "CamInfo(" + name + ", " + path + ", " + curve + ")";
        }
    }

    public struct ContactModel
    {
        double friction;
        double restitution;

        public override String ToString()
        {
            return "ContactModel(" + friction.ToString() + ", " + restitution.ToString() + ")";
        }
    }

    //ToDo: Research IPhysics Array structure
    public struct ConstraintInfo
    {
        String name;
        String itemA;
        String itemB;
        String owner;
        String type;
        double[] upperLimit;
        double[] lowerLimit;
        int[] axisUse;
        bool deactivateCollision;
        int direction;
        Trafo trafoA;
        Trafo trafoB;
        bool transient;

        public override String ToString()
        {
            return "ConstraintInfo(" + name + "," + itemA + "," + itemB + "," + owner + "," + type + "," + upperLimit.ToString() + ","
                + lowerLimit.ToString() + "," + axisUse.ToString() + "," + deactivateCollision.ToString() + "," + direction.ToString() + ","
                + direction.ToString() + "," + trafoA.ToIPhysicsString() + "," + trafoB.ToIPhysicsString() + "," + transient.ToString() + ")";
        }
    }

    public struct DriveInfo
    {
        DriveType type;
        double acceleration;
        double deceleration;
        double velocity;
        double target;
        bool forward;
        bool backward;
        double amplitude;
        double frequency;
        double phaseoffset;

        public override String ToString()
        {
            int typeVal = (int)type;
            return "DriveInfo(" + typeVal.ToString() + "," + acceleration.ToString() + "," + deceleration.ToString() + "," + velocity.ToString() + "," + forward.ToString() + ","
                + backward.ToString() + "," + amplitude.ToString() + "," + frequency.ToString() + "," + phaseoffset.ToString() + ")";
        }
    }

    public struct EmissionInfo
    {
        Vec3 startingVector;
        double emittedTypeOverride;
        double mass;
        bool stickToDynamicsOnly;
        bool prototypeGeneratorMode;
        bool opacityOverride;

        public override String ToString()
        {
            return "EmissionInfo(" + startingVector.ToIPhysicsString() + "," + emittedTypeOverride.ToString() + "," + mass.ToString() + "," + stickToDynamicsOnly.ToString() + ","
                + prototypeGeneratorMode.ToString() + "," + opacityOverride.ToString() + ")";
        }
    }

    public struct ForceDriveInfo
    {
        double initValue;
        Vec3 localOffsetA;
        Vec3 localOffsetB;
        double axisA;
        double axisB;

        public override String ToString()
        {
            return "ForceDriveInfo(" + initValue.ToString() + "," + localOffsetA.ToIPhysicsString() + "," + localOffsetB.ToIPhysicsString() + "," + axisA.ToString() + "," + axisB.ToString() + ")";
        }
    }

    public struct KinematicInfo
    {
        double limitEpsilon;
        KinematicAxis axis;
        double offset;
        double scale;
        String label;
        double minimum;
        double maximum;
        bool useResetValue;
        double resetValue;

        public override String ToString()
        {
            int axisVal = (int)axis;
            return "KinematicInfo(" + limitEpsilon.ToString() + "," + axisVal.ToString() + "," + offset.ToString() + "," + scale.ToString() + "," + label.ToString() + "," + minimum.ToString() +
                "," + maximum.ToString() + "," + useResetValue.ToString() + "," + resetValue.ToString() + ")";
        }
    }

    //ToDo: Research IPhysics Array structure
    public struct MotionControlInfo
    {
        bool deleteIt;
        int[] axes;

        public override string ToString()
        {
            return "MotionControlInfo(" + deleteIt.ToString() + "," + axes.ToString() + ")";
        }
    }

    public struct MovieEncoderSettings
    {
        double playbackSpeed;
        int targetFPS;
        int quality;
        int adjustWidth;
        int adjustHeight;
        double timeSize;
        int alignment;
        MovieEncoderFlags flags;

        public override string ToString()
        {
            int flagVal = (int)flags;
            return "MovieEncoderSettings(" + playbackSpeed.ToString() + "," + targetFPS.ToString() + "," + quality.ToString() + "," + adjustWidth.ToString() + "," +
                adjustHeight.ToString() + "," + timeSize.ToString() + "," + flagVal.ToString() + ")";
        }
    }

    public struct SpeedDriveInfo
    {
        SpeedDriveType type;
        double acceleration;
        double deceleration;
        double velocity;
        double target;
        bool forward;
        bool backward;

        public override string ToString()
        {
            int typeVal = (int)type;
            return "SpeedDriveInfo(" + typeVal.ToString() + "," + acceleration.ToString() + "," + deceleration.ToString() + "," + velocity.ToString() + "," +
                target.ToString() + "," + forward.ToString() + "," + backward.ToString() + ")";
        }
    }

    public struct TransientSourceInfo
    {
        bool respectIR;
        double releaseTime;
        bool useTrigger;

        public override string ToString()
        {
            return "TransientSourceInfo(" + respectIR.ToString() + "," + releaseTime.ToString() + "," + useTrigger.ToString() + ")";
        }
    }

    public class Trafo
    {
        Trafo()
        {
        }

        public String ToIPhysicsString()
        {
            return "print('Trafo Class is not yet implemented, please try to use Vec3 instead')";
        }
    }

    public class RenderMaterial
    {
        private Color diffuse;
        private Color ambient;
        private Color emission;
        private Color specular;

        public RenderMaterial(Color ambient, Color diffuse, Color emission, Color specular)
        {
            this.diffuse = diffuse;
            this.ambient = ambient;
            this.emission = emission;
            this.specular = specular;
        }

        public RenderMaterial(Color ambient, Color diffuse, Color emission)
        {
            this.diffuse = diffuse;
            this.ambient = ambient;
            this.emission = emission;
            this.specular = emission;
        }

        public RenderMaterial(Color ambient, Color diffuse)
        {
            this.diffuse = diffuse;
            this.ambient = ambient;
            this.emission = diffuse;
            this.specular = diffuse;
        }

        public RenderMaterial(Color ambient)
        {
            this.diffuse = ambient;
            this.ambient = ambient;
            this.emission = ambient;
            this.specular = ambient;
        }

        public String ToIPhysicsString()
        {
            return ("RenderMaterial(" + ambient.ToIPhysicsString() + ", " + diffuse.ToIPhysicsString() + ", " + emission.ToIPhysicsString() + ", " + specular.ToIPhysicsString() + ")");
        }
    }

    public class Color
    {
        public int r;
        public int g;
        public int b;
        public int a;

        public Color(int r, int g, int b, int a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(int r, int g, int b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 255;
        }

        public Color(int grey)
        {
            this.r = grey;
            this.g = grey;
            this.b = grey;
            this.a = 255;
        }

        public Color()
        {
            this.r = 255;
            this.g = 255;
            this.b = 255;
            this.a = 255;
        }

        public override String ToString()
        {
            return (r.ToString() + ", " + g.ToString() + ", " + b.ToString() + ", " + a.ToString());
        }

        public String ToIPhysicsString()
        {
            return ("Color(" + r.ToString() + ", " + g.ToString() + ", " + b.ToString() + ", " + a.ToString() + ")");
        }
    }

    public class mat4x4
    {

    }

    public class Vec3
    {
        public double x;
        public double y;
        public double z;

        private NumberFormatInfo nfi = new NumberFormatInfo();

        public Vec3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            nfi.NumberDecimalSeparator = ".";
        }

        public Vec3(Vec3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
            nfi.NumberDecimalSeparator = ".";
        }

        public Vec3()
        {
            x = 0;
            y = 0;
            z = 0;
            nfi.NumberDecimalSeparator = ".";
        }

        public override String ToString()
        {
            return (x.ToString() + ", " + y.ToString() + ", " + z.ToString());
        }

        public String ToIPhysicsString()
        {
            return ("Vec3("+x.ToString(nfi) + ", " + y.ToString(nfi) + ", " + z.ToString(nfi)+")");
        }
    }
}
