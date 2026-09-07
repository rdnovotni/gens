# UI virtualization notes

Ticket 5 deliberately does not claim large-list scalability. `ScrollView`
owns a viewport and offsets while its content owns logical extent and child
creation. A future `VirtualList` or `VirtualStackPanel` can therefore replace
the content node without changing input, focus, clipping, or scrolling APIs.

The proposed virtual panel owns the item source, item-template callback,
estimated/measured row extents, and a recycling pool. It receives the viewport
and offsets from its nearest scroll owner, realizes only the visible range plus
a small overscan, and exposes the full logical extent. Realized row nodes remain
ordinary single-parent `UiNode` instances; item identity belongs to the
presentation model, not the node name. Focused/captured rows must be retained
or transferred deliberately during recycling.

Before using it for production data, add a 10,000-row test that normally
realizes roughly 20–50 rows, covers variable heights and keyboard focus, and
proves scrolling never constructs the full item set.

