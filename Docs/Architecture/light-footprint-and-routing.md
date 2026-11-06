# Light footprint and waypoint routing

This contract connects the [spotlight rail](spotlight-rail.md) to form switching, shadow stability, and locomotion for the [first playable greybox](../Design/first-playable-greybox.drawio.svg). The dimensions and sample point below are proposed greybox defaults.

## Light footprint

- Build an axis-aligned rectangle in world XY from the latest `SpotlightRail.WorldPosition`. Start with a configurable width of **6 Unity units**, centered on the lamp's world X. The top is the lamp's world Y. Set the depth in the Inspector so the lower edge extends below the walkable floor.
- Use the rectangle to draw the visible gameplay boundary and to test illumination. Points on the boundary count as lit. Walls leave the footprint intact for this build.
- Sample the character at a **foot anchor** shared by both forms. Place waypoints at the corresponding foot positions. A route segment is lit when both endpoints are inside the same rectangle, since the rectangle is convex.
- Use one post-update footprint throughout each gameplay update. Form eligibility, shadow stability, and route selection all read that same footprint.

## Waypoint graph and target selection

Author waypoints and bidirectional walking links in the greybox scene. Each waypoint has a stable order, world foot position, and permitted forms. Validate each link against the same form-specific blockers used by character collision, including collider clearance along the walk segment. Use the authored graph for grounded movement. Airborne characters enter `Falling` and reevaluate when they land.

1. Start from the character's current foot position. At a waypoint, use that node as the start. Partway along a link, test the portions from the current position to its two endpoints using the active form's traversal rules below. On landing between nodes, reconnect only through a valid local walk segment on the landed surface. If no connection is valid, return `No valid route`.
2. Search the reachable nodes using the rules below. Among reachable lit nodes, pick the one with the smallest horizontal distance to the lamp center. Keep the current goal if it is still eligible and tied for best. Use the stable waypoint order for any remaining tie. Breadth-first traversal is sufficient for the small authored graph.
3. If the selected goal has already been reached, return `At goal` so locomotion enters `Idle`. A reachable goal with movement remaining enters `Following`. If there is no eligible goal, enter `Blocked` with `No valid route`.

| Active form | Goal eligibility | Traversal |
| --- | --- | --- |
| Physical | Reachable lit node permitted for Physical | A form-valid route can cross dark space. |
| Shadow | Reachable lit node permitted for Shadow | The character's current foot position and every remaining segment must stay lit. If the character is outside the footprint, clear steering immediately and report `Outside light` while the stability timer runs. |

When the lamp moves mid-link, check the remaining segment from the current foot position. This allows the light to leave a waypoint the character has already passed. Reevaluate on light movement, a successful form switch, landing, arrival, or route invalidation. Replace a route in place while `Following` stays active. [Character locomotion state machine](UML/character-locomotion-state-machine.drawio.svg)

## Integration order

1. While `Playing`, apply the pointer update to the rail and capture the resulting footprint.
2. Use that footprint for any shift request. On success, apply the new form and collision rules before resolving movement. Update shadow stability from the same foot sample and active form. [Character form state machine](UML/character-form-state-machine.drawio.svg)
3. Resolve the route and hand the result to locomotion before steering. When the shadow returns to light, clear the countdown and reevaluate the route. On timeout, let the level session handle failure. On restart, restore the light and rebuild the footprint before initializing locomotion. [Level session state machine](UML/level-session-state-machine.drawio.svg)

## Greybox acceptance

| Scenario | Expected result |
| --- | --- |
| A · Move the lamp from X 0 to X 4 with the character at spawn. | Physical walks from dark space toward the lit goal at X 4. |
| B · At X 4 in Physical, aim the lamp at X 6. Shift to Shadow, then advance the lamp to X 8 once the character reaches X 6. | Physical stops before the wall. Shadow reaches X 6, where shifting back is denied, then reaches X 8. At X 8, switch to Physical and guide the character through the Shadow Barrier via X 10.5 to X 12. |
| C · Shift in light at X 15 and move the lamp to X 17, 19, 21, 23, 25, then 27. Wait for the character at each waypoint. | Each remaining walk segment is lit. The player crosses the wall and reaches the exit. |
| C · Move the lamp from X 17 to X 23 while the character is at X 17 in Shadow. | Steering stops and the stability countdown starts. Restoring light before expiry lets the character recover. Letting the timer expire resets the level. |
| Move the light while the character is partway along a link, or leave two reachable goals equally close to the lamp. | The planner starts from the actual foot position, and an eligible tied goal remains stable. |

## First playable handoff

- The gameplay owner can implement the footprint and a visible boundary against the rail output before wiring character movement.
- The main scene owner can place the greybox markers and links in parallel. Integrate route selection and the A–C checks in the **13 November First Playable** build.
