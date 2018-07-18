import { IDGenerator } from '../src/idgen';

describe('IDGenerator', () => {
  it('next id', () => {
    const idgen = IDGenerator(10, 10);
    expect(idgen.next().value).toBe(10);
    expect(idgen.next().value).toBe(11);
    expect(idgen.next().value).toBe(12);
    expect(idgen.next().value).toBe(13);
  });

  it('loop range', () => {
    const idgen = IDGenerator(10, 3);
    expect(idgen.next().value).toBe(10);
    expect(idgen.next().value).toBe(11);
    expect(idgen.next().value).toBe(12);
    expect(idgen.next().value).toBe(10);
  });
});
