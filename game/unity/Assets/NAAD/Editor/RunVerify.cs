using UnityEditor;

namespace NAAD.Editor
{
    public static class RunVerify
    {
        [MenuItem("NAAD/Editor/RunVerify/Execute")]
        public static void Execute()
        {
            NAADBuild.Verify();
        }
    }
}
