using System.Collections.Generic;
using UnityEngine;
namespace AstralPuzzles {
// A self-contained standalone prototype. No player controller or UI package required.
public class PuzzleArcade : MonoBehaviour {
    enum Room { Menu, Dial, Momentum, Elements, Mirrors, Memory }
    Room room;
    readonly bool[] completed = new bool[5];
    readonly string[] titles = {"Astral Dial", "Trial of Momentum", "Elemental Convergence", "Shattered Reflection", "Forgotten Steps"};
    readonly string[] roman = {"I","II","III","IV","V","VI","VII","VIII","IX","X","XI","XII"};
    readonly List<int> entry = new List<int>();
    readonly int[] answer = {2,7,4,1}, elementOrder = {1,2,3,0}, memory = {0,2,1,3,2};
    readonly string[] elements = {"Fire","Water","Earth","Wind"};
    string feedback="";
    bool solved, running;
    int progress;
    float clock, memoryClock;
    bool watching, accepting;
    int lit=-1;
    Vector2 player, destination;
    readonly Vector2[] fragments = { new Vector2(120,110), new Vector2(470,100), new Vector2(470,300) };
    readonly bool[] taken = new bool[3];
    readonly Rect[] walls = { new Rect(210,35,30,225), new Rect(350,140,30,225) };
    readonly Vector2Int[] mirrorCells = {new Vector2Int(3,4),new Vector2Int(3,1),new Vector2Int(6,1)};
    readonly bool[] slashes = new bool[3];
    readonly List<Vector2> beam = new List<Vector2>();
    float beamHold;
    static readonly Rect board = new Rect(36,218,600,390);
    void Awake() {
        Application.targetFrameRate=60;
        // Keep Unity's Game view clear even when the minimal scene has no camera.
        if(Camera.allCamerasCount==0) {
            var display=new GameObject("Arcade Display").AddComponent<Camera>();
            display.transform.SetParent(transform,false);
            display.clearFlags=CameraClearFlags.SolidColor;
            display.backgroundColor=GardenPixels.Ink;
            display.cullingMask=0;
        }
    }
    void Begin(Room next) {
        room=next; solved=false; running=false; progress=0; entry.Clear(); feedback="";
        watching=false; accepting=false; lit=-1; clock=30; beamHold=0;
        player=destination=new Vector2(50,320);
        for(int i=0;i<taken.Length;i++) taken[i]=false;
        for(int i=0;i<slashes.Length;i++) slashes[i]=false;
        if(room==Room.Mirrors) TraceBeam();
    }
    void Win() { if(solved)return; solved=true; running=false; accepting=false; completed[(int)room-1]=true; feedback="A forgotten corner of the kingdom blooms again."; }
    void Update() {
        if(room==Room.Memory && watching) {
            memoryClock+=Time.deltaTime;
            int step=Mathf.FloorToInt(memoryClock/.9f);
            if(memoryClock<0) { lit=-1; }
            else if(step>=memory.Length) { watching=false; accepting=true; lit=-1; feedback="Your turn. Follow the garden song."; }
            else lit=memoryClock% .9f < .6f ? memory[step] : -1;
        }
        if(room==Room.Mirrors && !solved) { if(TraceBeam()) {beamHold+=Time.deltaTime; if(beamHold>=.5f)Win();} else beamHold=0; }
        if(room!=Room.Momentum || !running || solved)return;
        clock=Mathf.Max(0,clock-Time.deltaTime);
        if(clock<=0) { running=false; feedback="The bell rings. Try another run through the ruins."; return; }
        Vector2 next=Vector2.MoveTowards(player,destination,Mathf.Min(4,180*Time.deltaTime));
        Rect body=new Rect(next.x-10,next.y-10,20,20);
        bool blocked=false; foreach(Rect wall in walls) if(body.Overlaps(wall)) blocked=true;
        if(!blocked)player=next; else destination=player;
        for(int i=0;i<3;i++) if(Vector2.Distance(player,fragments[i])<24)taken[i]=true;
        Vector2 hazard=new Vector2(300,195+Mathf.Sin(Time.time*2)*140);
        if(Vector2.Distance(player,hazard)<24) {player=destination=new Vector2(50,320); feedback="The wandering star sent you back. Timer is still running.";}
        if(new Rect(540,300,50,75).Contains(player)) {
            if(taken[0]&&taken[1]&&taken[2])Win(); else feedback="The gate needs all three golden seeds.";
        }
    }
    bool TraceBeam() {
        beam.Clear(); Vector2Int cell=new Vector2Int(0,4), direction=Vector2Int.right;
        beam.Add(Center(cell));
        var seen=new HashSet<string>();
        for(int step=0;step<100;step++) {
            cell+=direction;
            if(cell.x<0||cell.x>=9||cell.y<0||cell.y>=6) {beam.Add(Center(cell));return false;}
            beam.Add(Center(cell));
            if(cell==new Vector2Int(6,4))return true;
            string state=cell+"/"+direction; if(!seen.Add(state))return false;
            for(int m=0;m<3;m++) if(cell==mirrorCells[m]) {
                direction=slashes[m] ? new Vector2Int(-direction.y,-direction.x) : new Vector2Int(direction.y,direction.x);
                break;
            }
        }
        return false;
    }
    Vector2 Center(Vector2Int cell) {return new Vector2(66+cell.x*60,255+cell.y*54);}
    Texture2D garden;
    readonly string[] subtitles={"THE CELESTIAL COURTYARD","A DASH THROUGH THE RUINS","FOUR LITTLE GUARDIANS","THE SUNBEAM CONSERVATORY","THE GARDEN REMEMBERS"};
    readonly string[] descriptions={"Read the riddle. Turn the ancient seal.","Gather starlight before the bell rings.","Wake the statues in nature's own order.","Bend a little sunshine toward a crystal.","Watch the stones. Follow their song."};
    void OnGUI() {
        float scale=Mathf.Min(Screen.width/960f,Screen.height/720f);
        GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-960*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
        if(!garden){garden=Resources.Load<Texture2D>("GardenBackdrop");if(garden)garden.filterMode=FilterMode.Point;}
        GardenPixels.Fill(new Rect(0,0,960,720),GardenPixels.Deep);
        if(garden)GUI.DrawTexture(new Rect(0,0,960,720),garden,ScaleMode.StretchToFill);
        for(int i=0;i<16;i++) {
            float x=60+(i*137)%840, y=110+(i*79)%530+Mathf.Round(Mathf.Sin(Time.time*.7f+i)*7);
            GardenPixels.Fill(new Rect(x,y,2,2),GardenPixels.Gold);
        }
        if(room==Room.Menu) {DrawMenu();return;}
        GardenPixels.Panel(new Rect(28,20,904,90));
        GardenPixels.Text(new Rect(48,37,630,24),"ASTRAL GARDENS / "+subtitles[(int)room-1],1,GardenPixels.Sky);
        GardenPixels.Text(new Rect(48,64,650,32),titles[(int)room-1],3,GardenPixels.Cream);
        if(GardenPixels.Button(new Rect(746,40,162,44),"GARDEN MAP")){Begin(Room.Menu);return;}
        GardenPixels.Panel(new Rect(28,127,904,61));
        GardenPixels.Text(new Rect(48,147,870,32),descriptions[(int)room-1],2,GardenPixels.Cream);
        GardenPixels.Panel(new Rect(28,207,620,410));
        GardenPixels.Panel(new Rect(674,207,258,410));
        GardenPixels.Text(new Rect(694,230,220,25),"FIELD NOTES",2,GardenPixels.Gold);
        GardenPixels.Fill(new Rect(694,258,216,2),GardenPixels.Moss);
        string note="";
        switch(room) {
        case Room.Dial: note="Two watch the gate. Seven wander above. Four mark the seasons. One lights the way.";DrawDial();break;
        case Room.Elements:note="Rain feeds the soil. From the soil rises breath. Breath feeds the flame.";DrawElements();break;
        case Room.Memory:note="Observe the glowing stones. Then repeat their order. You may listen again whenever you like.";DrawMemory();break;
        case Room.Mirrors:note="Click each mirror to turn it. Bring the sunbeam to the blue crystal. Keep it lit for a moment.";DrawMirrors();break;
        case Room.Momentum:note="Click to move. Go around the mossy walls. Gather three golden seeds and reach the green gate. Beware the wandering star!";DrawMomentum();break;
        }
        GardenPixels.Text(new Rect(694,282,216,205),note,2,GardenPixels.Cream);
        if(!solved && room!=Room.Momentum && GardenPixels.Button(new Rect(694,552,216,42),"START AGAIN")){Begin(room);return;}
        if(solved) {
            GardenPixels.Sprite("flower",new Vector2(802,481),3);
            GardenPixels.Text(new Rect(694,516,216,25),"GARDEN AWAKENED",2,GardenPixels.Gold,true);
            if(GardenPixels.Button(new Rect(694,552,216,42),"NEXT GARDEN",true,true)){Begin(Room.Menu);return;}
        }
        GardenPixels.Panel(new Rect(28,635,904,60));
        GardenPixels.Sprite(solved?"flower":"seed",new Vector2(54,665),2);
        GardenPixels.Text(new Rect(82,653,822,40),string.IsNullOrEmpty(feedback)?"A little curiosity opens every door.":feedback,2,GardenPixels.Cream);
    }
    void DrawMenu() {
        GardenPixels.Panel(new Rect(170,32,620,113));
        GardenPixels.Text(new Rect(190,52,580,20),"FIVE SMALL WONDERS IN A FORGOTTEN KINGDOM",1,GardenPixels.Sky,true);
        GardenPixels.Text(new Rect(190,81,580,40),"ASTRAL GARDENS",4,GardenPixels.Cream,true);
        GardenPixels.Text(new Rect(140,166,680,30),"CHOOSE A LITTLE ADVENTURE",2,GardenPixels.Cream,true);
        for(int i=0;i<5;i++) {
            float x=i<3?120+i*244:242+(i-3)*244, y=i<3?218:414;
            Rect card=new Rect(x,y,230,174);GardenPixels.Panel(card);
            GardenPixels.StoneTile(new Rect(x+14,y+14,202,76));
            string icon=i==0?"star":i==1?"player":i==2?"earth":i==3?"crystal":"flower";
            GardenPixels.Sprite(icon,new Vector2(x+115,y+53),4);
            GardenPixels.Text(new Rect(x+20,y+103,190,35),titles[i],2,GardenPixels.Cream,true);
            if(GardenPixels.Button(new Rect(x+20,y+141,190,24),completed[i]?"BLOOMED / REPLAY":"EXPLORE",true,completed[i],1))Begin((Room)(i+1));
        }
        int count=0;foreach(bool c in completed)if(c)count++;
        GardenPixels.Panel(new Rect(242,622,476,52));
        GardenPixels.Text(new Rect(256,640,448,25),count+" OF 5 GARDENS AWAKENED",2,GardenPixels.Cream,true);
    }
    void Floor(Rect area) {
        GardenPixels.Fill(area,GardenPixels.Deep);
        for(int y=0;y<6;y++)for(int x=0;x<10;x++) {
            float xx=area.x+x*60,yy=area.y+y*65;
            GardenPixels.Fill(new Rect(xx+2,yy+2,56,61),GardenPixels.Hex((x+y)%2==0?"285047":"2c564a"));
            GardenPixels.Fill(new Rect(xx+5,yy+5,15,2),GardenPixels.Teal);
            if((x*3+y)%5==0)GardenPixels.Fill(new Rect(xx+42,yy+48,5,3),GardenPixels.Moss);
        }
    }
    void DrawDial() {
        Floor(board);
        Vector2 center=new Vector2(336,405);
        for(int ring=0;ring<5;ring++) {
            int radius=148-ring*5;
            for(int p=0;p<64;p++) {
                float a=p*Mathf.PI*2/64;
                GardenPixels.Fill(new Rect(center.x+Mathf.Round(Mathf.Cos(a)*radius/3)*3,center.y+Mathf.Round(Mathf.Sin(a)*radius/3)*3,6,6),ring%2==0?GardenPixels.Gold:GardenPixels.Stone);
            }
        }
        GardenPixels.Sprite("star",new Vector2(336,383),4);
        GardenPixels.Text(new Rect(245,440,180,30),"THE ASTRAL SEAL",1,GardenPixels.Cream,true);
        for(int i=0;i<12;i++) {
            float angle=(i+1)*Mathf.PI/6-Mathf.PI/2;
            Rect r=new Rect(center.x+Mathf.Round(Mathf.Cos(angle)*145)-27,center.y+Mathf.Round(Mathf.Sin(angle)*145)-20,54,40);
            if(GardenPixels.Button(r,roman[i],!solved)) {
                entry.Add(i+1);
                if(entry.Count==4) {bool correct=true;for(int j=0;j<4;j++)correct &= entry[j]==answer[j];if(correct)Win();else{entry.Clear();feedback="The seal hums softly. Try another sequence.";}}
            }
        }
        for(int i=0;i<4;i++) {
            GardenPixels.Panel(new Rect(694+i*53,465,47,42));
            GardenPixels.Text(new Rect(698+i*53,480,39,20),i<entry.Count?roman[entry[i]-1]:"-",1,GardenPixels.Cream,true);
        }
        if(!solved&&GardenPixels.Button(new Rect(694,510,216,30),"CLEAR SEQUENCE",true,false,1))entry.Clear();
    }
    void DrawElements() {
        Floor(board);
        for(int i=0;i<4;i++) {
            float x=56+i*145;
            GardenPixels.StoneTile(new Rect(x,422,126,84));
            GardenPixels.Sprite(elements[i].ToLowerInvariant(),new Vector2(x+63,374),6);
            if(GardenPixels.Button(new Rect(x,515,126,48),elements[i],!solved)) {
                if(i==elementOrder[progress]){progress++;feedback="A guardian wakes: "+progress+" of 4.";if(progress==4)Win();}
                else{progress=0;feedback="The guardians doze off. Begin with the rain.";}
            }
        }
        for(int i=0;i<4;i++)GardenPixels.Sprite(i<progress?"flower":"seed",new Vector2(270+i*46,265),2);
    }
    void DrawMemory() {
        Floor(board);
        if(GardenPixels.Button(new Rect(210,239,250,42),watching?"LISTENING...":"OBSERVE / REPLAY",!solved)) {
            memoryClock=-.5f;watching=true;accepting=false;progress=0;lit=-1;feedback="Watch the garden's little song.";
        }
        for(int i=0;i<4;i++) {
            float x=70+i*140,y=345+(i%2)*55;
            Rect tile=new Rect(x,y,112,94);
            bool click=GUI.Button(tile,GUIContent.none,GUIStyle.none);
            GardenPixels.StoneTile(tile,lit==i);
            GardenPixels.Sprite(i==0?"flower":i==1?"star":i==2?"water":"earth",new Vector2(x+56,y+43),3);
            GardenPixels.Text(new Rect(x,y+104,112,22),(i+1).ToString(),2,GardenPixels.Cream,true);
            if(click&&accepting&&!solved) {
                if(i==memory[progress]){progress++;feedback="Footsteps remembered: "+progress+" of 5.";if(progress==5)Win();}
                else{accepting=false;feedback="A tiny stumble. Observe the song and try again.";}
            }
        }
        GardenPixels.Text(new Rect(80,565,510,25),watching?"WATCH THE GLOWING STONES":accepting?"YOUR TURN TO FOLLOW THE SONG":"OBSERVE FIRST, THEN FOLLOW",1,GardenPixels.Gold,true);
    }
    void DrawMirrors() {
        Floor(board);
        for(int y=0;y<6;y++)for(int x=0;x<9;x++) {
            Vector2 c=Center(new Vector2Int(x,y));
            GardenPixels.Fill(new Rect(c.x-25,c.y-22,50,44),GardenPixels.Deep);
            GardenPixels.Fill(new Rect(c.x-23,c.y-20,46,2),GardenPixels.Teal);
        }
        for(int i=1;i<beam.Count;i++)PixelLine(beam[i-1],beam[i],GardenPixels.Gold,6);
        for(int i=1;i<beam.Count;i++)PixelLine(beam[i-1],beam[i],GardenPixels.Cream,2);
        for(int i=0;i<3;i++) {
            Vector2 c=Center(mirrorCells[i]);Rect r=new Rect(c.x-24,c.y-22,48,44);
            if(GUI.Button(r,GUIContent.none,GUIStyle.none)&&!solved)slashes[i]=!slashes[i];
            GardenPixels.StoneTile(r);
            Vector2 a=c+new Vector2(-15,slashes[i]?15:-15),b=c+new Vector2(15,slashes[i]?-15:15);
            PixelLine(a,b,GardenPixels.Ink,10);PixelLine(a,b,GardenPixels.Sky,6);PixelLine(a,b,GardenPixels.Cream,2);
        }
        GardenPixels.Sprite("star",Center(new Vector2Int(0,4)),2);
        GardenPixels.Sprite("crystal",Center(new Vector2Int(6,4)),3);
        GardenPixels.Text(new Rect(94,566,470,25),"TURN THE MIRRORS. LET THE SUNSHINE IN.",1,GardenPixels.Gold,true);
    }
    void DrawMomentum() {
        Floor(board);
        foreach(Rect wall in walls) {
            Rect r=Offset(wall);GardenPixels.Fill(r,GardenPixels.Ink);
            for(int y=0;y<7;y++)GardenPixels.StoneTile(new Rect(r.x,r.y+y*32,r.width,32));
            GardenPixels.Fill(new Rect(r.x,r.y,r.width,5),GardenPixels.Moss);
        }
        Rect gate=Offset(new Rect(540,300,50,75));GardenPixels.Panel(gate);GardenPixels.Sprite("earth",gate.center,2);
        for(int i=0;i<3;i++)if(!taken[i])GardenPixels.Sprite("seed",board.position+fragments[i],2);
        Vector2 h=new Vector2(300,195+Mathf.Sin(Time.time*2)*140);
        GardenPixels.Sprite("fire",board.position+h,2);
        GardenPixels.Sprite("player",board.position+player,2);
        if(running) {GardenPixels.Fill(new Rect(board.x+destination.x-3,board.y+destination.y-3,6,6),GardenPixels.Sky);}
        GardenPixels.Text(new Rect(694,474,216,24),"TIME: "+clock.ToString("0.0"),2,clock<8?GardenPixels.Coral:GardenPixels.Gold);
        GardenPixels.Text(new Rect(694,505,216,24),"SEEDS: "+CountTaken()+" / 3",2,GardenPixels.Gold);
        if(!solved&&GardenPixels.Button(new Rect(694,552,216,42),running?"RETRY TRIAL":"START TRIAL")) {Begin(Room.Momentum);running=true;feedback="Follow the winding path. Collect every golden seed.";}
        if(Event.current.type==EventType.MouseDown&&board.Contains(Event.current.mousePosition)&&running) {
            destination=Event.current.mousePosition-board.position;
            destination.x=Mathf.Clamp(destination.x,10,590);destination.y=Mathf.Clamp(destination.y,10,380);Event.current.Use();
        }
    }
    void PixelLine(Vector2 a,Vector2 b,Color color,int width) {
        int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)/2));
        for(int i=0;i<=steps;i++){Vector2 p=Vector2.Lerp(a,b,(float)i/steps);GardenPixels.Fill(new Rect(Mathf.Round(p.x/2)*2-width/2,Mathf.Round(p.y/2)*2-width/2,width,width),color);}
    }

    int CountTaken(){int count=0;foreach(bool value in taken)if(value)count++;return count;}
    Rect Offset(Rect r){r.position+=board.position;return r;}
}
}

