using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearStaticDataManager : MonoBehaviour {
    private void Awake() {
        CuttingCounter.ClearStaticData();
        BaseCounter.ClearStaticData();
        TrashCounter.ClearStaticData();
    }
}

