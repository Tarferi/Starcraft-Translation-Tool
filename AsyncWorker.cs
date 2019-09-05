using System;
using System.ComponentModel;

namespace TranslatorUI {

    class AsyncJob {
        private Func<object, object> asyncRunCB;
        private object asyncRunParam;
        private Action<object> syncEndCB;

        private object result;

        private void asyncStart(object sender, DoWorkEventArgs e) {
            this.result = asyncRunCB(asyncRunParam);
        }

        private void syncEnd(object sender, RunWorkerCompletedEventArgs e) {
            syncEndCB(this.result);
        }

        public AsyncJob(Func<object, object> asyncRunCB, object asyncRunParam, Action<object> syncEndCB) {
            this.asyncRunCB = asyncRunCB;
            this.asyncRunParam = asyncRunParam;
            this.syncEndCB = syncEndCB;
        }

        public void start() {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(asyncStart);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(syncEnd);
            bw.RunWorkerAsync();
        }
    }

    class AsyncWorker {

        public void addJob(Func<object, object> asyncRun, object asyncRunParam, Action<object> syncEnd) {
            AsyncJob job = new TranslatorUI.AsyncJob(asyncRun, asyncRunParam, syncEnd);
            job.start();
        }
    }
}
