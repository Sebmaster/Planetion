using System.Collections.Generic;
using Planetion.Objects;

namespace Planetion.Utils {
	public class GroupedList<T> : List<T> where T : GameObject {

		public new void Add(T item) {
			bool found = false;

			foreach (Component c in item.Components.Values) {
				if (found)
					break;

				if (c is RenderableComponent) {
					RenderableComponent render = ((RenderableComponent)c);
					if (render.Identifier != null) {
						for (int i=0; i < this.Count && !found; ++i) {
							foreach (Component comp in this[i].Components.Values) {
								if (comp.GetType() == render.GetType()) {
									RenderableComponent renderCmp = (RenderableComponent)comp;

									if (renderCmp.Identifier == render.Identifier) {
										Insert(i, item);
										found = true;
										break;
									}
								}
							}
						}
					}
				}
			}

			if (!found) {
				base.Add(item);
			}
		}
	}
}

