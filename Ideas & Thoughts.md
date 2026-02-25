***Jump Abilities:*** I believe they use a sort of NavMesh Link that is instantiated on ability activation. It looks for the max distance assigned (maxJumpDistance) and if it hits then it performs the jump to that location. If not the jumpDistance will shrink until it hits a valid NavMesh point, then it will perform the jump

