using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;


/*
 * Thanks AngryAnt for a greate drag'n'drop example at this page http://angryant.com/2009/09/18/gui-drag-drop/
 * 
 */



public class UFTAtlasEditor : EditorWindow {
	[SerializeField]
	private UFTAtlas uftAtlas;
	
	private bool isAtlasDirty=false;
	public Vector2 scrollPosition = Vector2.zero;
	public Vector2 atlasTexturesScrollPosition=Vector2.zero;
	
	
	
	[MenuItem ("Window/Free Atlas Editor")]
    static void ShowWindow () {    		
		EditorWindow.GetWindow <UFTAtlasEditor>();				
    }
	
	void OnEnable(){
		uftAtlas=UFTAtlas.CreateInstance<UFTAtlas>();
		UFTAtlasEditorEventManager.onNeedToRepaint+=onNeedToRepaint;
		UFTAtlasEditorEventManager.onAtlasChange+=onAtlasChange;		
		UFTAtlasEditorEventManager.onAddNewEntry+=onAddNewEntry;
		UFTAtlasEditorEventManager.onRemoveEntry+=onRemoveEntry;
		UFTAtlasEditorEventManager.onStopDragging+=onStopDragging;
		UFTAtlasEditorEventManager.onStartDragging+=onStartDragging;
		UFTAtlasEditorEventManager.onTextureSizeChanged+=onTextureSizeChanged;
		foreach (UFTAtlasEntry uftAtlasEntry in uftAtlas.atlasEntries) {
			Undo.RegisterUndo(uftAtlas,"UFTAtlasEntry"+uftAtlasEntry.id);	
		}
		

		
	}

	public void onAddNewEntry (UFTAtlasEntry uftAtlasEntry)
	{		
		Undo.RegisterUndo(uftAtlasEntry,"UFTAtlasEntry" + uftAtlasEntry.id);
	}

	public void onStopDragging (UFTAtlasEntry uftAtlasEntry)
	{
		//Undo.SetSnapshotTarget(uftAtlasEntry,"stop dragging uftAtlasEntry id="+uftAtlasEntry.id);
	}

	public void onStartDragging (UFTAtlasEntry uftAtlasEntry)
	{
		registerSnapshotTargetState (uftAtlasEntry);
	}

	public void onRemoveEntry (UFTAtlasEntry uftAtlasEntry)
	{		
		registerSnapshotTargetState (uftAtlasEntry);
	}

	void registerSnapshotTargetState (UFTAtlasEntry uftAtlasEntry)
	{
		registerSnapshotForEntry (uftAtlasEntry);
		
	}

	void onTextureSizeChanged (UFTAtlasEntry uftAtlasEntry)
	{
		registerSnapshotForEntry (uftAtlasEntry);
	}

	void registerSnapshotForEntry (UFTAtlasEntry uftAtlasEntry)
	{
		Undo.SetSnapshotTarget(uftAtlas,"atlas");
		Undo.CreateSnapshot();
		Undo.RegisterSnapshot();
		
		Undo.SetSnapshotTarget(uftAtlasEntry,"stop dragging uftAtlasEntry id="+uftAtlasEntry.id);
		Undo.CreateSnapshot();
		Undo.RegisterSnapshot();
	}
	
		
	void Update(){
		Repaint();	
	}

	private void onAtlasChange ()
	{
		isAtlasDirty=true;
		Undo.CreateSnapshot();
        Undo.RegisterSnapshot();
	}
	
	
	private void onNeedToRepaint(){
		Repaint ();
	}	
	
	void OnGUI () {				
		if (isAtlasDirty){
			EditorUtility.SetDirty(uftAtlas);
			isAtlasDirty=false;
		}
		EditorGUILayout.Separator();
		EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.BeginVertical (new GUILayoutOption[]{GUILayout.Width(250f)});
				EditorGUILayout.Separator();		
				EditorGUILayout.LabelField("Atlas:");
				EditorGUILayout.TextField("name","");
				EditorGUILayout.BeginHorizontal();
		
					if (GUILayout.Button("new"))
						onClickNew();		
					if (GUILayout.Button("save"))
						onClickSave();
				EditorGUILayout.EndHorizontal();
		
				UFTAtlasSize newWidth = (UFTAtlasSize) EditorGUILayout.EnumPopup("width",uftAtlas.atlasWidth);
				UFTAtlasSize newHeight = (UFTAtlasSize) EditorGUILayout.EnumPopup("height",uftAtlas.atlasHeight);	
				
					
				
				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Allign:");
		
				uftAtlas.borderSize=EditorGUILayout.IntField("border size",uftAtlas.borderSize);
				if (GUILayout.Button("allign") && uftAtlas!=null && uftAtlas.atlasEntries.Count>0)
					uftAtlas.arrangeEntriesUsingUnityPackager();
				
				
				
				if (newWidth!=uftAtlas.atlasWidth || newHeight!=uftAtlas.atlasHeight){
					uftAtlas.atlasWidth=newWidth;
					uftAtlas.atlasHeight=newHeight;
					
					if (UFTAtlasEditorEventManager.onAtlasSizeChanged!=null)
						UFTAtlasEditorEventManager.onAtlasSizeChanged((int)newWidth,(int)newHeight);
				}
		
				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Atlas Textures:");
				if (uftAtlas.atlasEntries.Count>0){
					if (GUILayout.Button("trim alpha")){
						uftAtlas.trimAllEntries();				
					}					
				}
				atlasTexturesScrollPosition = EditorGUILayout.BeginScrollView(atlasTexturesScrollPosition);
				
				if (uftAtlas.atlasEntries.Count >0){
					List<string> names= uftAtlas.atlasEntries.ConvertAll(new System.Converter<UFTAtlasEntry,string>(uftAtlasEntryToString));
					names.Sort();
					foreach (string name in names) {
						EditorGUILayout.LabelField(name,GUILayout.MaxWidth(220f));
					}					
				}		
				EditorGUILayout.EndScrollView();
				EditorGUILayout.Separator();
			EditorGUILayout.EndVertical();
			
				
			EditorGUILayout.BeginVertical();	
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
					
					scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
						
							
					GUILayoutUtility.GetRect((int)uftAtlas.atlasWidth,(int)uftAtlas.atlasHeight);
					uftAtlas.OnGUI();
				
					EditorGUILayout.EndScrollView();
					EditorGUILayout.Separator();
				EditorGUILayout.EndHorizontal();				
			EditorGUILayout.EndVertical();
			
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Separator();
		
		 if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform){
	        DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // show a drag-add icon on the mouse cursor
			if(Event.current.type==EventType.dragPerform)
				HandleDroppedObjects(DragAndDrop.objectReferences);
            
			DragAndDrop.AcceptDrag();
	        Event.current.Use();
	    }		
	}



	void HandleDroppedObjects (Object[] objectReferences)
	{
		bool addedSomething=false;
		foreach (Object item in objectReferences) {
			if(typeof(Texture2D)==item.GetType()){
				Texture2D texture=(Texture2D)item;
				addedSomething=true;
				
				if (isTextureCanvasContainsTexture (texture)){
					Debug.Log("one of texture is already on Canvas");
				} else {
					string assetPath=AssetDatabase.GetAssetPath(texture);					
					uftAtlas.addNewEntry(texture,assetPath);
						
				}
				
			}
		}
		
		if (!addedSomething)
			Debug.Log("there was no any Texture2D in dropped content");
	}

	void onClickSave ()
	{
		UFTAtlasMetadata metadata=uftAtlas.getAtlasMetadata("testName");
		AssetDatabase.CreateAsset(metadata,"Assets/testName.asset");
		
	}

	void onClickNew ()
	{
		throw new System.NotImplementedException ();
	}

	
	
	
	private bool isTextureCanvasContainsTexture (Texture2D texture)
	{
		return uftAtlas.atlasEntries.Find(toc => toc.texture == texture)!=null;
	}
	
	
	
	// this function used as converter in OnGUI to show list of the entry names
	private static string uftAtlasEntryToString(UFTAtlasEntry uftAtlasEntry){
		return uftAtlasEntry.name;
	}
	
}
