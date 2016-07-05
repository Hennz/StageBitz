using StageBitz.Data;
using StageBitz.Logic.Business;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace StageBitz.Logic
{
    /// <summary>
    /// Builder class for BLs. (Bulder pattern)
    /// </summary>
    public class BLBuilder
    {
        /// <summary>
        /// Gets the BL list. (create all available BL objects using reflections)
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static Dictionary<Type, BaseBL> GetBLList(StageBitzDB dataContext)
        {
            Assembly logicDll = Assembly.GetExecutingAssembly();
            Dictionary<Type, BaseBL> blList = new Dictionary<Type, BaseBL>();
            blList.Add(typeof(BaseBL), new BaseBL(dataContext));
            List<Type> blTypes = logicDll.GetTypes().Where(t => t.BaseType == typeof(BaseBL)).ToList();
            foreach (Type blType in blTypes)
            {
                object instance = Activator.CreateInstance(blType, dataContext);
                blList.Add(blType, (BaseBL)instance);
            }

            return blList;
        }
    }
}
