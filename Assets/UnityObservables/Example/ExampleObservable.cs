using UnityEngine;

// Need to bring in the UnityObservables namespace
using UnityObservables;


public class ExampleObservable : MonoBehaviour {

    // Unity doesnt serialize generic types, so we must make concrete types for any observables
    // we want to use. Remember to give it the Serializable attribute.
    [System.Serializable]
    public class ObservableColor : Observable<Color> { }


    // Create the observable color field. It's possible to set the default color to red.
    public ObservableColor MyColor = new ObservableColor() { Value = Color.red };


    void Awake() {
        // Subscribe to the Observers OnChanged event. There is also a 'OnChangedValues' event which 
        // passes the previous and next values
        MyColor.OnChanged += ColorChangedHandler;
    }


    void ColorChangedHandler() {
        // The Observables current value is accessible through its 'Value' property. You can set a new
        // value with it too.
        GetComponent<Renderer>().material.color = MyColor.Value;
    }


    void OnValidate() {
        // Required to make the Observable fire events due to UNDO operations in Unity. If you're not
        // fussed about this then its not needed.
        MyColor.OnValidate();    
    }
}
