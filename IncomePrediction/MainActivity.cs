using System;
using System.Collections.Generic;

using Android.App;
using Android.Widget;
using Android.OS;

using Android.Content;
using Android.Runtime;
using Android.Views;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace IncomePrediction
{
    [Activity(Label = "Income Prediction", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            EditText age = FindViewById<EditText>(Resource.Id.age);
            EditText hoursperweek = FindViewById<EditText>(Resource.Id.hoursperweek);
            Button predictButton = FindViewById<Button>(Resource.Id.PredictButton);

            Spinner education = FindViewById<Spinner>(Resource.Id.education);
            education.Prompt="Select Education";
            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.education_array, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            education.Adapter = adapter;


            Spinner maritalstatus = FindViewById<Spinner>(Resource.Id.maritalstatus);
            maritalstatus.Prompt = "Select Marital Status";
            var adapter2 = ArrayAdapter.CreateFromResource(this, Resource.Array.maritalstatus_array, Android.Resource.Layout.SimpleSpinnerItem);
            adapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            maritalstatus.Adapter = adapter2;


            Spinner race = FindViewById<Spinner>(Resource.Id.race);
            race.Prompt = "Select Race";
            var adapter3 = ArrayAdapter.CreateFromResource(this, Resource.Array.race_array, Android.Resource.Layout.SimpleSpinnerItem);
            adapter3.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            race.Adapter = adapter3;

            Spinner sex = FindViewById<Spinner>(Resource.Id.sex);
            sex.Prompt = "Select Sex";
            var adapter4 = ArrayAdapter.CreateFromResource(this, Resource.Array.sex_array, Android.Resource.Layout.SimpleSpinnerItem);
            adapter4.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            sex.Adapter = adapter4;

            predictButton.Click += (Object sender, EventArgs e) =>
            {
                List<string> colNames = new List<string> {"age","education","marital-status","race","sex","hours-per-week"};
                List<string> colVals = new List<string> {
                    age.Text,
                    education.GetItemAtPosition(education.SelectedItemPosition).ToString(),
                    maritalstatus.GetItemAtPosition(maritalstatus.SelectedItemPosition).ToString(),
                    race.GetItemAtPosition(race.SelectedItemPosition).ToString(),
                    sex.GetItemAtPosition(sex.SelectedItemPosition).ToString(),
                    hoursperweek.Text
                };
                InvokeRequestResponseService(colNames, colVals);
            };

            async void InvokeRequestResponseService(List<string> colNames, List<string> colValues)
            {
                const string apiKey = "kmYh1fTF6WMp59kZ6WJu3+sijgWCLFZBRiMBduU4UVHG5/VI3GKpm8wCpoZL7Vp0w2pKENDY3LqmYsCN5hSefg=="; // Replace this with the API key for the web service
                const string apiUrl = "https://asiasoutheast.services.azureml.net/workspaces/50edb6832d9e4932bc7493e8fbc9e5c5/services/c7e2c1c108694ba1844e9e987970d38a/execute?api-version=2.0&details=true";

                //Column names and values
                StringTable stringTable = new StringTable();
                stringTable.ColumnNames = colNames.ToArray();

                int i = 0;
                int rowCnt = 1;//only a single row of input
                stringTable.Values = new string[rowCnt, colValues.Count];
                foreach (string item in colValues)
                {
                    stringTable.Values[0, i] = item;
                    i++;
                }
                //call the API
                using (var client = new HttpClient())
                {
                    var scoreRequest = new
                    {
                        Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            stringTable
                        },
                    },
                        GlobalParameters = new Dictionary<string, string>()
                        { }
                    };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    var uri = new Uri(apiUrl);

                    var json = JsonConvert.SerializeObject(scoreRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        //Get the result
                        string result = await response.Content.ReadAsStringAsync();
                        //Refine the output
                        if (result.Contains("<=50K"))
                        {
                            result = "Predicted Income is Less than $50,000.";
                        }
                        else
                        {
                            result = "Predicted Income is More than $50,000.";
                        }
                        //Show it to user
                        var Dialog = new AlertDialog.Builder(this);
                        Dialog.SetMessage(result);
                        Dialog.SetNeutralButton("Ok", delegate { });

                        // Show the Result dialog and wait for response
                        Dialog.Show();
                    }
                    else
                    {
                        //Get response header info: includes the requert ID and the timestamp, which are useful for debugging the failure
                        string responseHeader = response.Headers.ToString();
                        //Get the content
                        string responseContent = await response.Content.ReadAsStringAsync();
                        //Show it to user
                        var Dialog = new AlertDialog.Builder(this);
                        Dialog.SetMessage(response.StatusCode + ": " + responseHeader + " " + responseContent);
                        Dialog.SetNeutralButton("Ok", delegate { });

                        // Show the Error dialog and wait for response
                        Dialog.Show();
                    }
                }
            }


    }
    }

    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
}

