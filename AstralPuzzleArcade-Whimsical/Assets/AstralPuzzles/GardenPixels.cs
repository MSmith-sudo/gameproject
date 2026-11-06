using System.Collections.Generic;
using UnityEngine;
namespace AstralPuzzles {
// Small original bitmap font and sprites; all pixels use nearest-neighbor filtering.
public static class GardenPixels {
    public static readonly Color Ink=Hex("142c2c"), Deep=Hex("173f3b"), Teal=Hex("286c60"), Moss=Hex("44854a"), Leaf=Hex("8ca64b"), Gold=Hex("edbf66"), Cream=Hex("ffe5a2"), Stone=Hex("a97548"), Coral=Hex("df6d67"), Sky=Hex("79bda9");
    static readonly Dictionary<char,Texture2D> glyphs=new Dictionary<char,Texture2D>();
    static readonly Dictionary<string,Texture2D> sprites=new Dictionary<string,Texture2D>();
    static readonly string alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,:!?/-+()'";
    static readonly string[] patterns={
        "01110/10001/10001/11111/10001/10001/10001","11110/10001/10001/11110/10001/10001/11110","01111/10000/10000/10000/10000/10000/01111","11110/10001/10001/10001/10001/10001/11110","11111/10000/10000/11110/10000/10000/11111","11111/10000/10000/11110/10000/10000/10000","01111/10000/10000/10111/10001/10001/01110","10001/10001/10001/11111/10001/10001/10001","11111/00100/00100/00100/00100/00100/11111","00111/00010/00010/00010/00010/10010/01100","10001/10010/10100/11000/10100/10010/10001","10000/10000/10000/10000/10000/10000/11111","10001/11011/10101/10101/10001/10001/10001","10001/11001/10101/10011/10001/10001/10001","01110/10001/10001/10001/10001/10001/01110","11110/10001/10001/11110/10000/10000/10000","01110/10001/10001/10001/10101/10010/01101","11110/10001/10001/11110/10100/10010/10001","01111/10000/10000/01110/00001/00001/11110","11111/00100/00100/00100/00100/00100/00100","10001/10001/10001/10001/10001/10001/01110","10001/10001/10001/10001/10001/01010/00100","10001/10001/10001/10101/10101/10101/01010","10001/10001/01010/00100/01010/10001/10001","10001/10001/01010/00100/00100/00100/00100","11111/00001/00010/00100/01000/10000/11111",
        "01110/10001/10011/10101/11001/10001/01110","00100/01100/00100/00100/00100/00100/01110","01110/10001/00001/00010/00100/01000/11111","11110/00001/00001/01110/00001/00001/11110","00010/00110/01010/10010/11111/00010/00010","11111/10000/10000/11110/00001/00001/11110","01110/10000/10000/11110/10001/10001/01110","11111/00001/00010/00100/01000/01000/01000","01110/10001/10001/01110/10001/10001/01110","01110/10001/10001/01111/00001/00001/01110",
        "00000/00000/00000/00000/00000/00110/00110","00000/00000/00000/00000/00110/00110/00100","00000/00110/00110/00000/00110/00110/00000","01110/10001/00001/00010/00100/00000/00100","00100/00100/00100/00100/00100/00000/00100","00001/00010/00010/00100/01000/01000/10000","00000/00000/00000/11111/00000/00000/00000","00000/00100/00100/11111/00100/00100/00000","00010/00100/01000/01000/01000/00100/00010","01000/00100/00010/00010/00010/00100/01000","00100/00100/00000/00000/00000/00000/00000"};
    public static Color Hex(string h) {Color c;ColorUtility.TryParseHtmlString("#"+h,out c);return c;}
    public static void Fill(Rect r,Color c) {Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
    public static void Text(Rect r,string text,int size,Color color,bool center=false) {
        int columns=Mathf.Max(1,Mathf.FloorToInt(r.width/(6*size)));string[] words=text.ToUpperInvariant().Split(' ');
        var lines=new List<string>();string current="";
        foreach(string word in words) { if(current.Length>0 && current.Length+word.Length+1>columns){lines.Add(current);current="";}current+=(current.Length>0?" ":"")+word; }lines.Add(current);
        float y=r.y;
        Color old=GUI.color;GUI.color=color;
        foreach(string line in lines) {
            float x=center?r.x+(r.width-(line.Length*6-1)*size)/2:r.x;
            foreach(char c in line) {
                int index=alphabet.IndexOf(c);
                if(index>=0) {
                    Texture2D texture;
                    if(!glyphs.TryGetValue(c,out texture)) {
                        texture=new Texture2D(5,7,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
                        string[] rows=patterns[index].Split('/');
                        for(int yy=0;yy<7;yy++)for(int xx=0;xx<5;xx++)texture.SetPixel(xx,6-yy,rows[yy][xx]=='1'?Color.white:Color.clear);
                        texture.Apply();glyphs[c]=texture;
                    }
                    GUI.DrawTexture(new Rect(x,y,5*size,7*size),texture);
                }
                x+=6*size;
            }
            y+=10*size;if(y>r.yMax)break;
        }
        GUI.color=old;
    }
    public static void Panel(Rect r,bool light=false) {
        Fill(new Rect(r.x+4,r.y+6,r.width,r.height),Ink);Fill(r,Ink);
        Fill(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),light?Stone:Teal);
        Fill(new Rect(r.x+6,r.y+6,r.width-12,r.height-12),light?Gold:Deep);
        Fill(new Rect(r.x+8,r.y+8,r.width-16,2),light?Cream:Sky);
        foreach(Vector2 p in new[]{new Vector2(r.x+5,r.y+5),new Vector2(r.xMax-9,r.y+5),new Vector2(r.x+5,r.yMax-9),new Vector2(r.xMax-9,r.yMax-9)})Fill(new Rect(p.x,p.y,4,4),Gold);
    }
    public static bool Button(Rect r,string text,bool enabled=true,bool selected=false,int textSize=2) {
        bool hover=enabled&&r.Contains(Event.current.mousePosition);
        bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none)&&enabled;
        Color face=selected?Gold:(hover?Teal:Deep);
        Fill(new Rect(r.x,r.y+4,r.width,r.height),Ink);Fill(r,Ink);Fill(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),face);
        Fill(new Rect(r.x+3,r.y+3,r.width-6,3),selected?Cream:(hover?Sky:Moss));
        Text(new Rect(r.x+4,r.y+(r.height-7*textSize)/2,r.width-8,r.height),text,textSize,!enabled?Moss:(selected?Ink:Cream),true);
        return clicked;
    }
    public static void Sprite(string name,Vector2 center,int scale=3) {
        Texture2D texture;
        if(!sprites.TryGetValue(name,out texture)) {
            string[] rows;
            switch(name) {
            case "player": rows=new[]{"......1111......",".....155551.....","....15555551....","...1555555551...","..111111111111..",".....166661.....",".....160061.....",".....166661.....","....13333331....","....13333331....","...1333333331...","...1333333331...","....11111111....",".....11..11.....","....111..111....","................"};break;
            case "crystal": rows=new[]{".......1........","......141.......",".....14441......","....1446441.....","...144464441....","...144464441....","...144464441....","...144464441....","....1446441.....",".....14641......","......141.......",".......1........",".....11111......","....1222221.....","...122222221....","...111111111...."};break;
            case "fire": rows=new[]{".......3........","......333.......",".....3333.......",".....35333......","....3353333.....","...335553333....","...355655533....","...355655533....","....3555533.....",".....33333......","......111.......","....1222221.....","....1262621.....","....1222221.....","...122222221....","...111111111...."};break;
            case "water": rows=new[]{".......4........","......444.......","......464.......",".....44644......","....4446444.....","....4446444.....","...444464444....","...444644444....","....4444444.....",".....44444......","......111.......","....1222221.....","....1262621.....","....1222221.....","...122222221....","...111111111...."};break;
            case "earth": rows=new[]{"................",".....77777......","....7778777.....","...777787777....","...788888887....","....7778777.....",".....77877......","......787.......","......787.......","......222.......","......111.......","....1222221.....","....1262621.....","....1222221.....","...122222221....","...111111111...."};break;
            case "wind": rows=new[]{"................",".....44444......","....46....4.....",".....4..........","...444444444....","..46.......44...","...44444....4...",".......444444...",".....44.........","......4444......","......111.......","....1222221.....","....1262621.....","....1222221.....","...122222221....","...111111111...."};break;
            case "star":rows=new[]{".......5........",".......5........","......555.......","......565.......",".....55655......","..55555655555...","...555666555....","....5566655.....",".....55655......","....5555555.....","....55...55.....","...55.....55....","................","................","................","................"};break;
            case "flower":rows=new[]{"................","................",".....33..33.....","....33333333....","....33355333....",".....355553.....",".....355553.....","....33355333....","....33333333....",".....33..33.....",".......7........","....77.7........",".....777.77.....",".......777......",".......7........","................"};break;
            default:rows=new[]{"......1111......",".....155551.....","....15566551....","...1556666551...","..155666666551..","..155666666551..","...1556666551...","....15566551....",".....155551.....","......1111......","................","................","................","................","................","................"};break;
            }
            Color[] palette={Color.clear,Ink,Stone,Coral,Sky,Gold,Cream,Moss,Leaf};
            texture=new Texture2D(16,16,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<16;y++)for(int x=0;x<16;x++)texture.SetPixel(x,15-y,rows[y][x]=='.'?Color.clear:palette[rows[y][x]-'0']);
            texture.Apply();sprites[name]=texture;
        }
        GUI.DrawTexture(new Rect(Mathf.Round(center.x)-8*scale,Mathf.Round(center.y)-8*scale,16*scale,16*scale),texture);
    }
    public static void StoneTile(Rect r,bool lit=false) {
        Fill(r,Ink);Fill(new Rect(r.x+3,r.y+3,r.width-6,r.height-9),lit?Gold:Stone);
        Fill(new Rect(r.x+6,r.y+6,r.width-12,3),lit?Cream:Gold);
        Fill(new Rect(r.x+6,r.yMax-9,r.width-12,3),lit?Stone:Deep);
        Fill(new Rect(r.x+3,r.yMax-6,r.width-6,3),Moss);
    }
}
}
