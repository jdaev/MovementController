using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; //All unity editor scripts need using UnityEditor, this means they CANNOT COMPILE. They must be placed in special Editor folder

//This script will be visible instead of the default editor script for the "target" MovementController
[CustomEditor(typeof(MovementController))]   //Define this script as an "Editor" script, which targets Movement Controller
public class MovementControllerEditor : Editor
{ //Extends Editor required
    private SerializedObject movementCntrl;    //A serialized object
    private SerializedProperty movementSpeed;  //A serialized variable
    private SerializedProperty wpArrayCount;  //A serialized variable

    public void OnEnable()
    {
        movementCntrl = new SerializedObject(target);          //target is MovementController
        movementSpeed = movementCntrl.FindProperty("speed");   //Use reflection to find a variable (property) in the target project
        wpArrayCount = movementCntrl.FindProperty(Ref_Array_Size_Path); //Get the array length
    }

    private static string Ref_Array_Size_Path = "waypoints.Array.size";
    private static string Ref_Array_Data_Path = "waypoints.Array.data[{0}]";

    private Transform[] GetWaypointArray()
    {
        //Find property "waypoints.Array.size" which grabs the field named waypoints, then accesses it as an Array, and grabs the getter in it "Size"
        int arrayCount = wpArrayCount.intValue;    // movementCntrl.FindProperty(Ref_Array_Size_Path).intValue;  Now we use our own int to track it
        Transform[] arr = new Transform[arrayCount];

        //We call  waypoints.Array.data[0], waypoints.Array.data[1]... to get the data, which is returned as an "object". (the base class of all things). Need to cast it to the correct type
        for (int i = 0; i < arrayCount; i++)
            arr[i] = GetWaypointAt(i);

        return arr;
    }

    public Transform GetWaypointAt(int index)
    {
        return movementCntrl.FindProperty(string.Format(Ref_Array_Data_Path, index)).objectReferenceValue as Transform;
    }


    public void SetWaypoint(int index, Transform waypoint)
    {
        movementCntrl.FindProperty(string.Format(Ref_Array_Data_Path, index)).objectReferenceValue = waypoint; //Find the array property and set it
    }

    public void AddWaypoint(Transform toAdd)
    {
        wpArrayCount.intValue++;                       //Increase the count (It is just an int)
        SetWaypoint(wpArrayCount.intValue - 1, toAdd); //Call set waypoint, setting the last new value to null
    }

    private void RemoveWaypointAtIndex(int index)
    {
        for (int i = index; i < wpArrayCount.intValue - 1; i++)                 //Starting at the index, move all of them down by one
            SetWaypoint(i, GetWaypointAt(i + 1));                               //Get the next waypoint and move it forward
        wpArrayCount.intValue -= 1;                                             //Decrease the count, which will remove the last one
    }

    private void DuplicateWaypointAtIndex(int index)
    {
        wpArrayCount.intValue++;
        for (int i = wpArrayCount.intValue - 1; i > index; i--)
        {
            SetWaypoint(i, GetWaypointAt(i - 1));
        }
    }

    private void Swap(int index1, int index2)
    {
        Transform t = GetWaypointAt(index1);
        SetWaypoint(index1, GetWaypointAt(index2));
        SetWaypoint(index2, t);
    }

    public override void OnInspectorGUI() //Must be overwritten, this is called everytime the mouse moves over the inspector
    {

        movementCntrl.Update();                                                //Stores relative variables from Unity Runtime into object

        GUILayout.Label("Movement Controller Params", EditorStyles.boldLabel); //Displays a label in editor with Bold Label 

        EditorGUILayout.PropertyField(movementSpeed);                          //Display the default format for a property field of type given
        if (movementSpeed.floatValue < 0)                                      //Is type SerializedProperty, which contains floatValue, intValue, ...
            movementSpeed.floatValue = 0;                                      //Set the property if is negative

        GUILayout.Label("Waypoints", EditorStyles.boldLabel);
        for (int i = 0; i < wpArrayCount.intValue; i++)
        {
            bool initialEnabledSetting = GUI.enabled;
            GUILayout.BeginHorizontal();                //Every GUI element created after this call will belong to the same horizontal line

            GUI.enabled = GetWaypointAt(i);
            if (GUILayout.Button("D", GUILayout.Width(25f)))
                DuplicateWaypointAtIndex(i);

            GUI.enabled = initialEnabledSetting; 
            
            Transform result = EditorGUILayout.ObjectField(GetWaypointAt(i), typeof(Transform), true) as Transform; //Make a field that can be set to any object, and returns the object in it

            if (GUI.changed)                            //If any gui values have changed this update
                SetWaypoint(i, result);                 //set the wp


            //Save the current state of the UI, it could be already disabled
            GUI.enabled = (i > 0) && initialEnabledSetting;
            if (GUILayout.Button("^", GUILayout.Width(20f)))
                Swap(i, i - 1);

            GUI.enabled = (i < wpArrayCount.intValue - 1) && initialEnabledSetting;
            if (GUILayout.Button("v", GUILayout.Width(20f)))
                Swap(i, i + 1);

            GUI.enabled = initialEnabledSetting;      //Reset the state of the UI! if you leave it disabled, it will disable everything that follows

            if (GUILayout.Button("-", GUILayout.Width(20f)))  //A button of width 20
                RemoveWaypointAtIndex(i);                     //Calls remove function on current for loop index (i)

            GUILayout.EndHorizontal();                  //Ends the horizontal line called earlier
        }
        DropAreaGUI();

        movementCntrl.ApplyModifiedProperties();        //Save all changes, should usaully be placed last line at the end
    }

    private void DropAreaGUI()
    {
        Event evt = Event.current;                                                              //Grab any event currently being processed
        Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));         //Create a flexible Rect object
        GUI.Box(dropArea, "Add waypoint");                                                      //Creates a GUI element that is a box, that takes a rect as an arg
        switch (evt.type)                                //What is the event type?
        {

            case EventType.DragUpdated:                 //Every frame a drag is occuring
            case EventType.DragPerform:                 //The frame when a drag is finished
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy; //Sets the plus sign on the cursor, apparently is required for drag modes

                if (evt.type == EventType.DragPerform)   //Was frame it was released
                {
                    DragAndDrop.AcceptDrag();           //Lets the event system know that this controller has handled the drag event
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        GameObject go = draggedObject as GameObject; //Cast the dragged object, if it was not a gameobject (a different script type or resource) ignore it
                        if (!go)
                            continue;

                        Transform t = go.transform;     //In theory can never happen, but imagine it was a different script type
                        if (!t)
                            continue;

                        AddWaypoint(t);
                    }

                }
                Event.current.Use();                    //States this event was used (So no other controllers use this event)
                break;
        }
    }

    private void OnSceneGUI() //Monobehaviour editor function
    {
        MovementController mc = target as MovementController;
        Handles.ArrowHandleCap(0, mc.transform.position, mc.transform.rotation, mc.speed * .3f, EventType.Repaint); //Draws an arrow, at pos in direction of a given size. EventType.Repaint means it happens every frame

        if (mc.waypoints.Length > 1)
        {
            Color originalHandleColor = Handles.color;                                                      //Like Enabled, we must perserve the original handle color!
            Handles.color = Color.green;                                                                    //Set the color of all 
            for (int i = 0; i < mc.waypoints.Length - 1; i++)                                               //Length minus one, because the last one doesnt have a "next"
                if (mc.waypoints[i] && mc.waypoints[i + 1])
                    Handles.DrawLine(mc.waypoints[i].position, mc.waypoints[i + 1].position);                   //Draw a line from i to i+1
            if(mc.waypoints[0] && mc.waypoints[mc.waypoints.Length - 1])
            Handles.DrawLine(mc.waypoints[mc.waypoints.Length - 1].position, mc.waypoints[0].position);     //Draw line from last element back to first element
            Handles.color = originalHandleColor;                                                            //Reset the handle back to the original!
        }

        Handles.BeginGUI();                                                                                 //Begin 2D gui handle, specifies that this is on the scene
        for (int i = 0; i < mc.waypoints.Length; i++)
        {
            if (!mc.waypoints[i])
                continue;
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(mc.waypoints[i].position);                     //Convert a space in the world to the 2D GUI space
            Rect rect = new Rect(guiPoint.x - 50f, guiPoint.y - 40, 100, 20);                               //Create a scalable rect (In this case, hardcoded size)
            GUI.Box(rect, "Waypoint: " + i);                                                                //Create a GUI text box with the rect size
        }
        Handles.EndGUI();                                                                                   //Call end to GUI 2D Handle
    }

}
