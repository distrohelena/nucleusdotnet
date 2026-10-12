using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus {
    public class LayeredList<T> {
        protected List<List<T>> layers;

        public LayeredList()
            : this(new List<T>()) {
        }

        public LayeredList(List<T> first) {
            layers = new List<List<T>>();

            // add first layer
            layers.Add(first);
        }

        public List<T> CurrentLayer {
            get {
                return layers[layers.Count - 1];
            }
        }

        /// <summary>
        /// Returns the count in the top layer
        /// </summary>
        public int Count {
            get {
                return layers[layers.Count - 1].Count;
            }
        }

        public int LayerCount {
            get { return layers.Count; }
        }

        public void PushLayer() {
            layers.Add(new List<T>());
        }

        public void PushLayer(List<T> dict) {
            layers.Add(dict);
        }

        public void PopLayer() {
            layers.RemoveAt(layers.Count - 1);
        }

        public void PopAll() {
            layers.Clear();
        }
    }
}
