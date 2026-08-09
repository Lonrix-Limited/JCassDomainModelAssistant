# .claude/ — placeholder

**Filled by session S11.**

Will hold Claude skills: thin wrappers over the `jcass-dm` verbs in [`../tools/`](../tools/) and
over reads of [`../docs/`](../docs/).

**A skill never holds knowledge that is not also in `docs/`.** The test is deliberately blunt:
delete this whole folder and the Assistant still works at full capability, with more typing.
Anything a skill does, an assistant that has never heard of skills must be able to do by reading
`docs/` and calling `jcass-dm`.
