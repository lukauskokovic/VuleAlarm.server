using System.IO;
public static class RPIO
{
    const string base_path = "/sys/class/gpio/";
    public static void Export(int pinId){
        if(!Directory.Exists(base_path + "gpio"+pinId))
            write(base_path + "export", pinId); 
    }
    public static void UnExport(int pinId) => write(base_path + "unexport", pinId);
    public static void SetDirection(int pinId, Direction direction) => write(string.Format("{0}gpio{1}/direction", base_path, pinId), direction == Direction.OUT? "out":"in");
    public static void SetOutput(int pinId, bool value) => write(string.Format("{0}gpio{1}/value", base_path, pinId), value?"1":"0");
    public static bool ReadInput(int pinId){
        string path = string.Format("{0}gpio{1}/value", base_path, pinId);
        if(!File.Exists(path)){
            System.Console.WriteLine("failed to find file for pin state");
            return false;
        }else{
            try{
                string file = File.ReadAllText(path);
                return file.Contains('1') ? true : false;
            }catch(System.Exception ex){
                System.Console.WriteLine("Failed to read file for pin state " +ex.Message);
                return false;
            }
        }
    }
    
    static void write(string path, object value){
        try{
            File.AppendAllText(path, value.ToString());
        }
        catch(System.Exception ex){
            System.Console.WriteLine("Failed to write to gpio file {0}\n{1}", path, ex.Message);
        }
    }

    public enum Direction
    {
        IN = 0,
        OUT = 1
    }
}