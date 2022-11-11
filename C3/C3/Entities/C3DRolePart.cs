namespace C3.Entities
{
    public class C3DRolePart
    {
        //Data

        protected uint idType;
        protected int Action;
        //protected CGame3DEffectEx objGame3DEffectEx; // Is this Super effect on weps?

        //For light
        int LightIndex;
        uint TotalCost;
        byte SAlpha;
        byte SRed;
        byte SGreen;
        byte SBlue;

        byte Alpha;
        byte Red;
        byte Green;
        byte Blue;
        //end for light??

        public C3DObjPartInfo? infoPart;

        //end data


        //public C3DRolePart() { }
        //~C3DRolePart() { }

        //public virtual void Destroy() { throw new NotImplementedException(); }
        //public virtual void Move(float x, float y, float z) { throw new NotImplementedException(); }
        //public virtual void Rotate(float x, float y, float z) { throw new NotImplementedException(); }
        //public virtual void Scale(float x, float y, float z) { throw new NotImplementedException(); }
        //public virtual void SetARGB(float alpha, float red, float green, float blue) { throw new NotImplementedException(); }
        //public virtual void NextFrame(int nStep) { throw new NotImplementedException(); }
        //public virtual void SetFrame(uint dwFrame) { throw new NotImplementedException(); }
        //public virtual void Draw(int type, float lightx, float lighty, float lightz, float sa, float sr, float sg, float sb) { throw new NotImplementedException(); }
        //public virtual void DrawAlpha(int type, float lightx, float lighty, float lightz, float sa, float sr, float sg, float sb, float height) { throw new NotImplementedException(); }
        //public virtual C3DObjPartInfo GetInfo(int nIndex) { throw new NotImplementedException(); }
        //public virtual int GetInfoAmount() { throw new NotImplementedException(); }
        //public virtual void Clear3DEffect(string? szIndex = null) { throw new NotImplementedException(); }
        //public virtual void AddEffect(string pszIndex, bool bOnlyOnce = false) { throw new NotImplementedException(); }
        //public virtual void SetMotion(C3DMotion pMotion) { throw new NotImplementedException(); }
        //public virtual void SetTexture() { throw new NotImplementedException(); }
        //public virtual void Set3DEffect(int nLook, int nAction, int nVariable = 0, CGame3DEffectEx* pEffect = null, string? pszVmesh = null) { throw new NotImplementedException(); }

        //public void SetVirtualMotion(C3Motion pMotion) { throw new NotImplementedException(); }
        //public void SetDefaultMotion() { throw new NotImplementedException(); }
        //public CGame3DEffectEx* QueryGame3DEffectEx() { return &this->m_objGame3DEffectEx; }

        //public void MuliplyPhy(D3DXMATRIXA16* pobjMatrix) { throw new NotImplementedException(); }
        //public void ClearMatrix() { throw new NotImplementedException(); }
        //public void TransformCoord(D3DXVECTOR3* pobjMin, D3DXVECTOR3* pobjMax, D3DXMATRIXA16* pobjMatrix) { throw new NotImplementedException(); }
        //public void TransformCoordforSimpleObj(D3DXVECTOR3* pobjMin, D3DXVECTOR3* pobjMax, D3DXMATRIXA16* pobjMatrix) { throw new NotImplementedException(); }
        //public C3Motion* GetVirtualMotion(string pszName) { throw new NotImplementedException(); }

        //public uint GetTypeID() { return idType; }
        //public void ProcessLight() { throw new NotImplementedException(); }
        //public void CreateLight(uint idLight) { throw new NotImplementedException(); }
    }
}
