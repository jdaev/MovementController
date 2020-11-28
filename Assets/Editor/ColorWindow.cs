using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;  //Required for MenuItem, means that this is an Editor script, must be placed in an Editor folder, and cannot be compiled!
using System.Linq;  //Used for Select

public class ColorWindow : EditorWindow
{ //Now is of type EditorWindow

    [MenuItem("Custom Tools/ Color Window")] //This the function below it as a menu item, which appears in the tool bar
    public static void CreateShowcase() //Menu items can call STATIC functions, does not work for non-static since Editor scripts cannot be attached to objects
    {
        EditorWindow window = GetWindow<ColorWindow>("Color Window");
    }

    private Color[,] colors;
    private int width = 8;
    private int height = 8;

    float randomnessBound = 0;
    private float colorComparisonTolerance = 0.1f;
    private bool fill = false;
    Texture colorTexture;
    Renderer textureTarget;

    Color selectedColor = Color.white;
    Color eraseColor = Color.white;

    public void OnEnable()
    {
        colors = new Color[height, width];
        for (int i = 0; i < colors.GetLength(0); i++)
            for (int j = 0; j < colors.GetLength(1); j++)
            {
                colors[i, j] = GetRandomColor();
            }

        colorTexture = EditorGUIUtility.whiteTexture;
    }

    private Color GetRandomColor()  //Built a get random color tool
    {
        return new Color(Random.value, Random.value, Random.value, 1f);
    }

    private Color GetVariatedColor(Color color)
    {
        float randomness = Random.Range(0, randomnessBound);
        Color variant = color;
        variant.r += color.r * randomness;
        variant.g += color.g * randomness;
        variant.b += color.b * randomness;

        return variant;
    }

    void OnGUI() //Called every frame in Editor window
    {
        GUILayout.BeginHorizontal();        //Have each element below be side by side
        DoControls();
        DoCanvas();
        GUILayout.EndHorizontal();
    }

    void DoControls()
    {
        GUILayout.BeginVertical();                                                      //Start vertical section, all GUI draw code after this will belong to same vertical
        GUILayout.Label("ToolBar", EditorStyles.largeLabel);                            //A label that says "Toolbar"
        randomnessBound = EditorGUILayout.FloatField("Random", randomnessBound);
        if (randomnessBound < 0)
            randomnessBound = 0;
        if (randomnessBound > 1)
            randomnessBound = 1;

        selectedColor = EditorGUILayout.ColorField("Paint Color", selectedColor);       //Make a color field with the text "Paint Color" and have it fill the selectedColor var
        fill = EditorGUILayout.Toggle("Fill", fill);
        eraseColor = EditorGUILayout.ColorField("Erase Color", eraseColor);             //Make a color field with the text "Erase Color"
        if (GUILayout.Button("Fill All"))                                               //A button, if pressed, returns true
        {
            for (int i = 0; i < colors.GetLength(0); i++)
                for (int j = 0; j < colors.GetLength(1); j++)
                {
                    colors[i, j] = selectedColor;
                }
        }

        GUILayout.FlexibleSpace();                                                      //Flexible space uses any left over space in the loadout
        textureTarget = EditorGUILayout.ObjectField("Output Renderer", textureTarget, typeof(Renderer), true) as Renderer;  //Build an object field that accepts a renderer

        if (GUILayout.Button("Save to Object"))
        {
            Texture2D t2d = new Texture2D(width, height);                               //Create a new texture
            t2d.filterMode = FilterMode.Point;                                          //Simplest non-blend texture mode
            textureTarget.material = new Material(Shader.Find("Diffuse"));              //Materials require Shaders as an arguement, Diffuse is the most basic type
            textureTarget.sharedMaterial.mainTexture = t2d;                             //sharedMaterial is the MAIN RESOURCE MATERIAL. Changing this will change ALL objects using it, .material will give you the local instance

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {

                    t2d.SetPixel(i, j, colors[i, j]);                     //Color every pixel using our color table, the texture is 8x8 pixels large, but strecthes to fit
                }
            }
            t2d.Apply();                                                                //Apply all changes to texture
        }
        GUILayout.EndVertical();                                                        //end vertical section
    }


    void DoCanvas()
    {
        Event evt = Event.current;                     //Grab the current event

        Color oldColor = GUI.color;                    //GUI color uses a static var, need to save the original to reset it
        GUILayout.BeginHorizontal();                   //All following gui will be on one horizontal line until EndHorizontal is called
        for (int i = 0; i < width; i++)
        {
            GUILayout.BeginVertical();                //All following gui will be in a vertical line
            for (int j = 0; j < height; j++)
            {
                int index = j + i * height;           //Rememeber, this is just like a 2D array, but in 1D
                Rect colorRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); //Reserve a square, which will autofit to the size given
                if ((evt.type == EventType.MouseDown || evt.type == EventType.MouseDrag) && colorRect.Contains(evt.mousePosition)) //Can now paint while dragging update
                {
                    if (evt.button == 0)                //If mouse button pressed is left
                    {
                        if (fill)
                        {
                            FloodFill(i, j, selectedColor, colors[i, j]);
                        }
                        else
                            colors[i, j] = GetVariatedColor(selectedColor);
                    }
                    else
                        colors[i, j] = GetVariatedColor(eraseColor);   //Set the color of the index
                    evt.Use();                        //The event was consumed, if you try to use event after this, it will be non-sensical
                }

                GUI.color = colors[i, j];            //Same as a 2D array
                GUI.DrawTexture(colorRect, colorTexture); //This is colored by GUI.Color!!!
            }
            GUILayout.EndVertical();                  //End Vertical Zone
        }
        GUILayout.EndHorizontal();                    //End horizontal zone
        GUI.color = oldColor;                         //Restore the old color
    }
    bool CompareColors(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < colorComparisonTolerance
            && Mathf.Abs(a.g - b.g) < colorComparisonTolerance
            && Mathf.Abs(a.b - b.b) < colorComparisonTolerance;
    }
    void FloodFill(int x, int y, Color fillColor, Color canvasColor)
    {
        if (x < 0 || x >= height || y < 0 || y >= width 
            || !CompareColors(canvasColor, colors[x, y]) 
            || CompareColors(colors[x, y], fillColor)
        )
        {

            return;

        }
        colors[x, y] = GetVariatedColor(fillColor);

        FloodFill(x + 1, y, fillColor, canvasColor);
        FloodFill(x - 1, y, fillColor, canvasColor);
        FloodFill(x, y + 1, fillColor, canvasColor);
        FloodFill(x, y - 1, fillColor, canvasColor);

    }
}
