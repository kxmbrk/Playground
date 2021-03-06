﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Playground.Core.AdoNet;

namespace Playground.WpfApp.Forms.TreeViewEx.TreeViewLib
{
    public class Database
    {
        public static async Task<List<MetaAccountModel>> LoadData()
        {
            return await Task.Run(async () =>
            {
                //Load all Categories
                var categoryDic = await GetDictionary(AcctType.Category);

                //get Accounts
                var accounts = await CategoryOrAccount(AcctType.Account);

                //insert accounts to category
                foreach (var acct in accounts)
                {
                    MetaAccountModel categoryItem;
                    categoryDic.TryGetValue(acct.ParentId.ToString(), out categoryItem);

                    if (categoryItem != null)
                    {
                        acct.SetParent(categoryItem);
                        categoryItem.ChildrenAdd(acct);
                    }
                }

                var categoryList = await CategoryOrAccount(AcctType.Category);

                //Create parent-child relationship on categories and return Category as the root level nodes.
                foreach (var item in accounts)
                {
                    var category = categoryList.FirstOrDefault(x => x.Id.Equals(item.ParentId));
                    if (category != null)
                    {
                        item.SetParent(category);
                        category.ChildrenAdd(item);
                    }
                }

                return categoryList;

            });
        }

        public static Task<List<MetaAccountModel>> CategoryOrAccount(AcctType type)
        {
            return Task.Run(() =>
            {
                List<MetaAccountModel> list = new List<MetaAccountModel>();
                DataTable dt;

                if (type == AcctType.Category)
                {
                    dt = DAL.Seraph.ExecuteQuery("SELECT * FROM ACCT_CATEGORY");
                    foreach (DataRow r in dt.Rows)
                    {
                        var categoryId = Convert.ToInt32(r["CATEGORY_ID"]);
                        string categoryName = r["CATEGORY_NAME"].ToString();

                        var item = new MetaAccountModel(null, categoryId, categoryName, 0, AcctType.Category);
                        list.Add(item);
                    }
                }
                else if (type == AcctType.Account)
                {
                    dt = DAL.Seraph.ExecuteQuery(@"
                        SELECT A.*,
                               B.CATEGORY_NAME
                        FROM ACCT_MGR A
                        INNER JOIN ACCT_CATEGORY B
                        ON A.CATEGORY_ID = B.CATEGORY_ID");
                    foreach (DataRow r in dt.Rows)
                    {
                        var acctId = Convert.ToInt32(r["ID"]);
                        var acctName = r["ACCT_NAME"].ToString();
                        int parentId = Convert.ToInt32(r["CATEGORY_ID"]);

                        var item = new MetaAccountModel(null, acctId, acctName, parentId, AcctType.Account);
                        list.Add(item);
                    }
                }

                return list;
            });
        }

        public static async Task<Dictionary<string, MetaAccountModel>> GetDictionary(AcctType type)
        {
            var list = await CategoryOrAccount(type);

            var dictionary = new Dictionary<string, MetaAccountModel>();

            foreach (var item in list)
                dictionary.Add(item.Id.ToString(), item);

            return dictionary;
        }
    }
}
