using System;
using System.Collections.Generic;
using SayKitInternal;

namespace SayKitInternalTest
{
    public class SayTest
    {

        public void SayTest_Start()
        {

            SayKit_trackRewardedOffer();
            SayKit_trackRewardedOffer2();

            SayKit_trackItem();
            SayKit_trackItem2();

            SayKit_trackItemLoss();
            SayKit_trackItemLoss2();




            SayKit_trackChunkStarted("test_chunk");
            SayKit_trackChunkCompleted();

            SayKit_trackChunkStarted("test_chunk_1");
            SayKit_trackChunkFailed();


            SayKit_trackChunkStarted2();
            SayKit_trackChunkCompleted2();

            SayKit_trackChunkStarted("test_chunk_f2");
            SayKit_trackChunkFailed2();


            SayKit_trackChunkStarted3();
            SayKit_trackChunkCompleted3();

            SayKit_trackChunkStarted("test_chunk_f3");
            SayKit_trackChunkFailed3();


            SayKit_trackChunkStarted("test_chunk_lc1");
            SayKit.trackLevelCompleted(1, 1);

            SayKit_trackChunkStarted("test_chunk_lf1");
            SayKit.trackLevelFailed(1, 1);

            SayKit_trackChunkStarted("test_chunk_lc2");
            SayKit.trackLevelStageCompleted(1, 1);

            SayKit_trackChunkStarted("test_chunk_lf2");
            SayKit.trackLevelStageFailed(1, 1);

            SayKit_trackChunkStarted("test_chunk_lf2_cf");
            SayKit.trackLevelStageFailed(1, 1);
            SayKit_trackChunkFailed();

            SayKit_trackChunkStarted("test_chunk_lc2_cc");
            SayKit.trackLevelStageCompleted(1, 1);
            SayKit_trackChunkCompleted();



            SayKit_trackTutorialStep("test_tutorial");

            //for (int i = 0; i < 1000; i++)
            //{
            //    SayKit_trackTutorialStep("test_tutorial_" + i);
            //}

            SayKit_trackTutorialStep2();
        }



        private void SayKit_trackRewardedOffer()
        {
            SayKit.trackRewardedOffer("test_offer");
        }

        private void SayKit_trackRewardedOffer2()
        {
            SayKit.trackRewardedOffer("ad_rewarded_buylevel");
            SayKit.trackRewardedOffer("ad_rewarded_buylevel");
        }




        private void SayKit_trackItem()
        {
            SayKit.trackItem("test_item", 5, SourceType.Free, null);
        }

        private void SayKit_trackItem2()
        {
            Dictionary<string, object> customData = new Dictionary<string, object>();
            customData.Add("test_int", 5);
            customData.Add("test_string", "str");

            SayKit.trackItem("test_item2", 5, SourceType.Free, customData);
        }



        private void SayKit_trackItemLoss()
        {
            SayKit.trackItem("test_item_loss", 5, SourceType.Free, null);
        }

        private void SayKit_trackItemLoss2()
        {
            Dictionary<string, object> customData = new Dictionary<string, object>();
            customData.Add("test_int", 5);
            customData.Add("test_string", "str");

            SayKit.trackItem("test_item_loss2", 5, SourceType.Free, customData);
        }



        private void SayKit_trackChunkStarted(string name)
        {
            SayKit.trackChunkStarted(name, 5);
        }

        private void SayKit_trackChunkStarted2()
        {
            SayKit.trackChunkStarted("test_chunk_s2", 5, null);
        }

        private void SayKit_trackChunkStarted3()
        {
            Dictionary<string, object> customData = new Dictionary<string, object>();
            customData.Add("test_int", 5);
            customData.Add("test_string3", "str");

            SayKit.trackChunkStarted("test_chunk_s3", 5, customData);
        }



        private void SayKit_trackChunkCompleted()
        {
            SayKit.trackChunkCompleted();
        }

        private void SayKit_trackChunkCompleted2()
        {
            SayKit.trackChunkCompleted(null);
        }

        private void SayKit_trackChunkCompleted3()
        {
            Dictionary<string, object> customData = new Dictionary<string, object>();
            customData.Add("test_int", 5);
            customData.Add("test_string3", "str");

            SayKit.trackChunkCompleted(customData);
        }



        private void SayKit_trackChunkFailed()
        {
            SayKit.trackChunkFailed();
        }

        private void SayKit_trackChunkFailed2()
        {
            SayKit.trackChunkFailed(null);
        }

        private void SayKit_trackChunkFailed3()
        {
            Dictionary<string, object> customData = new Dictionary<string, object>();
            customData.Add("test_int", 5);
            customData.Add("test_string3", "str");

            SayKit.trackChunkFailed(customData);
        }



        private void SayKit_trackTutorialStep(string name)
        {
            SayKit.trackTutorialStep(name, "test_step");
        }

        private void SayKit_trackTutorialStep2()
        {
            SayKit.trackTutorialStep("test_tutorial", "test_step_2");
            SayKit.trackTutorialStep("test_tutorial", "test_step_2");
            SayKit.trackTutorialStep("test_tutorial", "test_step_2");
        }



    }
}